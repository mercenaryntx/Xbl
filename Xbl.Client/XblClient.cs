using System.Net.Http.Json;
using System.Text.Json;
using Xbl.Client;
using Xbl.Models;

namespace Xbl;

public class XblClient
{
    private const string TitlesFile = "titles.json";
    private const string DataFolder = "data";

    private readonly HttpClient _client = new();
    private readonly IOutput _output;

    public XblClient(string apiKey, IOutput output)
    {
        if (!Directory.Exists(DataFolder)) Directory.CreateDirectory(DataFolder);

        _output = output;
        _client.DefaultRequestHeaders.Add("x-authorization", apiKey);
        _client.BaseAddress = new Uri("https://xbl.io/api/v2/");
    }

    public async Task Update(string update)
    {
        try
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Getting titles... ");
            await GetTitles();
            var titles = await LoadTitles();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("OK");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($" [{titles.Length}]");

            if (update is "all" or "achievements")
            {
                await UpdateTitles(titles, title => $"{title.TitleId}.json", "achievements", GetAchievements);
            }

            if (update is "all" or "stats")
            {
                await GetStatsBulk(titles);
                //await UpdateTitles(titles, title => $"{title.TitleId}.stats.json", "stats", GetStats);
            }
        }
        catch (HttpRequestException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Error: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("OpenXBL API returned an error ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"({(int?)ex.StatusCode}) {ex.StatusCode}");
        }
    }

    private static async Task UpdateTitles(IEnumerable<Title> titles, Func<Title, string> dataFile, string type, Func<string, Task> updateLogic)
    {
        var changes = titles.Where(title =>
        {
            var x = new FileInfo(Path.Combine(DataFolder, dataFile(title)));
            return title.TitleHistory.LastTimePlayed > x.LastWriteTimeUtc;
        }).ToArray();

        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("Found ");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write(changes.Length);
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($" {type} changes");

        for (var i = 0; i < changes.Length; i++)
        {
            var title = changes[i];
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($"[{i + 1:D3}/{changes.Length:D3}] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Updating ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(title.Name);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("... ");
            await updateLogic(title.TitleId);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("OK");
        }
    }

    public async Task RarestAchievements(int limit)
    {
        var titles = await LoadTitles();
        var data = titles
            .OrderByDescending(t => t.Achievement.ProgressPercentage)
            .ToDictionary(t => t, t => LoadAchievements(t.TitleId).GetAwaiter().GetResult());

        var rarest = data.SelectMany(kvp => kvp.Value.Where(a => a.ProgressState == "Achieved").Select(a => new RarestAchievementItem(
            kvp.Key.Name,
            a.Name,
            a.Rarity.CurrentPercentage
        ))).OrderBy(a => a.Percentage).Take(limit);

        _output.RarestAchievements(rarest);
    }

    public async Task MostComplete(int limit, IEnumerable<Title> additionalTitles = null)
    {
        IEnumerable<Title> titles = await LoadTitles();
        if (additionalTitles != null) titles = titles.Union(additionalTitles);

        var data = titles
            .OrderByDescending(t => t.Achievement?.ProgressPercentage)
            .ThenByDescending(t => t.Achievement?.TotalGamerscore)
            .Take(limit);

        _output.MostComplete(data);
    }

    public async Task SpentMostTimeWith(int limit)
    {
        var titles = await LoadTitles();

        var data = titles
            .OrderByDescending(t => t.Achievement?.ProgressPercentage)
            .ThenByDescending(t => t.Achievement?.TotalGamerscore)
            //.Take(149)
            .ToDictionary(t => t, t => LoadStats(t.TitleId).GetAwaiter().GetResult().FirstOrDefault(s => s.Name == "MinutesPlayed")?.IntValue ?? 0)
            .OrderByDescending(t => t.Value)
            .Take(limit)
            .Select(kvp => new MinutesPlayed(kvp.Key.Name, kvp.Value));

        _output.SpentMostTimeWith(data);
    }

    public async Task Count(IEnumerable<Title> additionalTitles = null)
    {
        var titles = await LoadTitles();
        RenderProfile(titles, "Live Profile");

        if (additionalTitles != null)
        {
            var a = additionalTitles.ToArray();
            RenderProfile(a, "Xbox 360 Profile");

            var allTitles = titles.Concat(a);
            var g = allTitles.GroupBy(t => t.Name).ToArray();
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("All titles: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(allTitles.Count());
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Unique titles: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(g.Length);
        }
    }

    private static void RenderProfile(Title[] titles, string prefix)
    {
        var c1 = titles.Length;
        var count = titles.Sum(t => t.Achievement?.CurrentAchievements);
        var sum = titles.Sum(t => t.Achievement?.CurrentGamerscore);
        var played = titles.Sum(t => LoadStats(t.TitleId).GetAwaiter().GetResult().FirstOrDefault(s => s.Name == "MinutesPlayed")?.IntValue ?? 0);

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"{prefix}:");
        RenderProfileLine(c1, "games");
        RenderProfileLine(count, "unlocked achievements");
        RenderProfileLine(sum, "gamerscore");
        RenderProfileLine($"{TimeSpan.FromMinutes(played).TotalHours:0.0}", "hours played");
        Console.WriteLine();
    }

    private static void RenderProfileLine(object value, string suffix)
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write(" - ");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write(value);
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine($" {suffix}");
    }

    #region Infra

    public async Task GetTitles()
    {
        var s = await _client.GetStringAsync("achievements/");
        await File.WriteAllTextAsync(Path.Combine(DataFolder, TitlesFile), s);
    }

    public static async Task<Title[]> LoadTitles()
    {
        var path = Path.Combine(DataFolder, TitlesFile);
        if (!File.Exists(path))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Error: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Data files cannot be found. Please run an update first");
            return Array.Empty<Title>();
        }

        var a = await JsonHelper.FromFile<AchievementTitles>(path);
        return a.Titles.OrderByDescending(t => t.Achievement.ProgressPercentage).ToArray();
    }

    public async Task GetAchievements(string titleId)
    {
        var s = await _client.GetStringAsync("achievements/title/" + titleId);
        await File.WriteAllTextAsync(Path.Combine(DataFolder, $"{titleId}.json"), s);
    }

    public static async Task<Achievement[]> LoadAchievements(string titleId)
    {
        var path = Path.Combine(DataFolder, $"{titleId}.json");
        if (!File.Exists(path)) return Array.Empty<Achievement>();

        var details = await JsonHelper.FromFile<TitleDetails>(path);
        return details.Achievements;
    }

    //public async Task GetStats(string titleId)
    //{
    //    var s = await _client.GetStringAsync("achievements/stats/" + titleId);
    //    await File.WriteAllTextAsync(Path.Combine(DataFolder, $"{titleId}.stats.json"), s);
    //}

    public async Task GetStatsBulk(IEnumerable<Title> titles)
    {
        //var playerJson = await _client.GetStringAsync("player/summary");
        //var player = JsonSerializer.Deserialize<Player>(playerJson);
        //var xuid = player.People.First().XUID;

        var changes = titles.Where(title =>
        {
            var x = new FileInfo(Path.Combine(DataFolder, $"{title.TitleId}.stats.json"));
            return title.TitleHistory.LastTimePlayed > x.LastWriteTimeUtc;
        }).ToArray();

        var a = await JsonHelper.FromFile<AchievementTitles>(Path.Combine(DataFolder, TitlesFile));
        var pages = changes.Chunk(100).Select(c => new PlayerStatsRequest
        {
            XUIDs = new[] { a.Xuid },
            Stats = c.Select(x => new Stat { Name = "MinutesPlayed", TitleId = x.TitleId }).ToArray()
        }).ToArray();

        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("Updating stats ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        var cursor = Console.GetCursorPosition();
        Console.Write("0%");

        var i = 0;
        foreach (var page in pages)
        {
            var response = await _client.PostAsJsonAsync("player/stats", page);
            var content = await response.Content.ReadAsStringAsync();
            var stats = JsonSerializer.Deserialize<TitleStats>(content);

            foreach (var stat in stats.StatListsCollection[0].Stats)
            {
                var titleStats = new TitleStats
                {
                    Groups = Array.Empty<TitleStatGroup>(),
                    StatListsCollection = new[]
                    {
                        new StatList
                        {
                            Stats = new[]
                            {
                                stat
                            }
                        }
                    }
                };
                var json = JsonSerializer.Serialize(titleStats);
                await File.WriteAllTextAsync(Path.Combine(DataFolder, $"{stat.TitleId}.stats.json"), json);
            }
            Console.SetCursorPosition(cursor.Left, cursor.Top);
            Console.Write($"{++i*100/pages.Length}%");
        }
        Console.WriteLine();
    }

    public static async Task<Stat[]> LoadStats(string titleId)
    {
        var path = Path.Combine(DataFolder, $"{titleId}.stats.json");
        if (!File.Exists(path))
        {
            return Array.Empty<Stat>();
        }

        var details = await JsonHelper.FromFile<TitleStats>(path);
        return details.StatListsCollection.Length == 0 ? Array.Empty<Stat>() : details.StatListsCollection[0].Stats;
    }

    #endregion
}