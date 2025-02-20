using System.Collections.Immutable;
using System.Net.Http.Json;
using System.Text.Json;
using AutoMapper;
using KustoLoco.Core;
using Spectre.Console;
using Xbl.Client.Mapping;
using Xbl.Client.Models;
using Xbl.Xbox360.Extensions;
using Xbl.Xbox360.Io.Gpd;
using Xbl.Xbox360.Io.Stfs;
using Xbl.Xbox360.Io.Stfs.Data;
using Xbl.Xbox360.Models;

namespace Xbl.Client;

public class XblClient
{
    private const string Live = "live";
    private const string Xbox360 = "x360";
    private const string DataFolder = "data";

    private readonly HttpClient _client = new();
    private readonly IOutput _output;
    private readonly int _limit;
    private readonly IMapper _mapper;
    private string _xuid;

    public XblSettings Settings { get; }

    public XblClient(XblSettings settings)
    {
        Settings = settings;
        _limit = settings.Limit == 0 ? 50 : settings.Limit;
        _output = settings.Output?.ToLower() switch
        {
            "json" => new XblJson(),
            _ => new XblConsole()
        };

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _client.DefaultRequestHeaders.Add("x-authorization", settings.ApiKey);
        _client.BaseAddress = new Uri("https://xbl.io/api/v2/");
    }

    #region Update

    public async Task<int> Update(string update)
    {
        try
        {
            AnsiConsole.Markup("[white]Getting titles... [/]");
            await GetLiveTitles();
            var titles = await LoadTitles(false);
            AnsiConsole.MarkupLineInterpolated($"[#16c60c]OK[/] [white][[{titles.Length}]][/]");

            if (update is "all" or "achievements")
            {
                var src = titles.Where(t => t.Achievement.CurrentAchievements > 0 && !t.IsMobile);
                var x360 = src.Where(t => t.IsXbox360);
                var rest = src.Where(t => !t.IsXbox360);
                await UpdateLiveTitles(rest, "Live achievements", GetAchievements);
                await UpdateLiveTitles(x360, "X360 achievements", Get360Achievements);
            }
            if (update is "all" or "stats")
            {
                await GetStatsBulk(titles);
            }

            return 0;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine();
            AnsiConsole.MarkupLineInterpolated($"[red]Error:[/] [silver]OpenXBL API returned an error [/] [red]({(int?)ex.StatusCode}) {ex.StatusCode}[/]");
            return -1;
        }
    }

    private static async Task UpdateLiveTitles(IEnumerable<Title> titles, string type, Func<Title, Task> updateLogic)
    {
        var changes = titles.Where(title =>
        {
            var x = new FileInfo(GetAchievementFilePath(title));
            return title.TitleHistory.LastTimePlayed > x.LastWriteTimeUtc;
        }).ToArray();

        AnsiConsole.MarkupLineInterpolated($"[white]Found[/] [cyan1]{changes.Length}[/] [white]{type} changes[/]");

        for (var i = 0; i < changes.Length; i++)
        {
            var title = changes[i];
            AnsiConsole.MarkupInterpolated($"[silver][[{i + 1:D3}/{changes.Length:D3}]][/] [white]Updating [/] [cyan1]{title.Name}[/][white]... [/]");
            await updateLogic(title);
            AnsiConsole.MarkupLine("[#16c60c]OK[/]");
        }
    }

    #endregion

    #region Import

    public async Task<int> Import()
    {
        AnsiConsole.MarkupInterpolated($"[white]Importing Xbox 360 profile...[/] ");
        var cursor = Console.GetCursorPosition();
        AnsiConsole.MarkupLine("[#f9f1a5]0%[/]");

        try
        {
            var profilePath = Settings.ProfilePath;
            var bytes = await File.ReadAllBytesAsync(profilePath);
            var profile = ModelFactory.GetModel<StfsPackage>(bytes);
            profile.ExtractGames();

            var profileHex = Path.GetFileName(profilePath);
            var titles = new AchievementTitles
            {
                Xuid = BitConverter.ToUInt64(profileHex.FromHex()).ToString(),
                Titles = GetTitlesFromProfile(profile)
            };

            var json = JsonSerializer.Serialize(titles);
            await SaveJson(GetTitlesFilePath(Xbox360), json);

            var i = 0;
            var n = 0;
            foreach (var (fileEntry, game) in profile.Games)
            {
                game.Parse();

                var achievements = new TitleDetails<Achievement>
                {
                    Achievements = GetAchievementsFromGameFile(fileEntry, game, out var hadBug)
                };
                if (hadBug) n++;

                json = JsonSerializer.Serialize(achievements);
                await SaveJson(GetAchievementFilePath(Xbox360, game.TitleId), json);

                Console.SetCursorPosition(cursor.Left, cursor.Top);
                AnsiConsole.MarkupLineInterpolated($"[#f9f1a5]{++i * 100 / profile.Games.Count}%[/]");
            }

            for (i = 0; i < n; i++)
            {
                Console.WriteLine();
            }

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLineInterpolated($"[red]Error:[/] [silver]Failed to import. {ex.Message}[/]");
            return -1;
        }
    }

    private static Title[] GetTitlesFromProfile(StfsPackage profile)
    {
        return profile
            .ProfileInfo
            .TitlesPlayed
            .Where(g => !string.IsNullOrEmpty(g.TitleName))
            .Select(g =>
            {
                var t = new Title
                {
                    TitleId = BitConverter.ToInt32(g.TitleCode.FromHex()).ToString(),
                    HexId = g.TitleCode,
                    Name = g.TitleName,
                    Type = "Game",
                    IsXbox360 = true,
                    Achievement = new AchievementSummary
                    {
                        CurrentAchievements = g.AchievementsUnlocked,
                        CurrentGamerscore = g.GamerscoreUnlocked,
                        ProgressPercentage = g.TotalGamerScore > 0 ? 100 * g.GamerscoreUnlocked / g.TotalGamerScore : 0,
                        TotalGamerscore = g.TotalGamerScore,
                        TotalAchievements = g.AchievementCount
                    }
                };

                return t;

            })
            .ToArray();
    }

    private static Achievement[] GetAchievementsFromGameFile(FileEntry fileEntry, GameFile game, out bool hadBug)
    {
        var bugReported = false;
        var achievements = game.Achievements.Select(a =>
        {
            try
            {
                if (a.Gamerscore < 0 || a.AchievementId < 0) throw new Exception();

                return new Achievement
                {
                    Id = a.AchievementId,
                    Name = a.Name,
                    TitleId = BitConverter.ToInt32(game.TitleId.FromHex()),
                    TitleName = game.Title,
                    Unlocked = a.IsUnlocked,
                    TimeUnlocked = a.IsUnlocked ? a.UnlockTime : DateTime.MinValue,
                    Platform = "Xbox360",
                    IsSecret = a.IsSecret,
                    Description = a.UnlockedDescription,
                    LockedDescription = a.LockedDescription,
                    Gamerscore = a.Gamerscore
                };
            }
            catch
            {
                if (bugReported) return null;
                AnsiConsole.MarkupLineInterpolated($"[#f9fba5]Warning:[/] {game.Title} ([grey]{fileEntry.Name}[/]) is corrupted. Invalid entries are omitted.");
                bugReported = true;
                return null;
            }
        }).Where(a => a != null).ToArray();

        hadBug = bugReported;
        return achievements;
    }

    #endregion

    #region Built-in queries

    public async Task RarestAchievements()
    {
        var titles = await LoadTitles(false);
        var data = titles
            .OrderByDescending(t => t.Achievement.ProgressPercentage)
            .ToDictionary(t => t, t => LoadAchievements(t).GetAwaiter().GetResult());

        var rarest = data.SelectMany(kvp => kvp.Value.Where(a => a.Unlocked).Select(a => new RarestAchievementItem(
            kvp.Key.Name,
            a.Name,
            a.Rarity.CurrentPercentage
        ))).OrderBy(a => a.Percentage).Take(_limit);

        _output.RarestAchievements(rarest);
    }

    public async Task WeightedRarity()
    {
        var titles = await LoadTitles(false);
        var data = titles
            .OrderByDescending(t => t.Achievement.ProgressPercentage)
            .ToDictionary(t => t, t => LoadAchievements(t).GetAwaiter().GetResult());

        var mostRare = data.Select(t => new WeightedAchievementItem(
            t.Key.Name,
            t.Key.Achievement,
            t.Value.Count(),
            t.Value.Count(a => a.Unlocked),
            t.Value.Count(a => a.Unlocked && a.Rarity.CurrentCategory == "Rare"),
            t.Value.Where(a => a.Unlocked)
                .Sum(a =>
                {
                    double score = a.Gamerscore;
                    var weightFactor = Math.Exp((100 - a.Rarity.CurrentPercentage) / 5.0);
                    return score / t.Key.Achievement.TotalGamerscore * weightFactor;
                })
        )).OrderByDescending(a => a.Weight).Take(_limit);

        _output.WeightedRarity(mostRare);
    }

    public async Task MostComplete()
    {
        IEnumerable<Title> titles = await LoadTitles();

        var data = titles
            .OrderByDescending(t => t.Achievement?.ProgressPercentage)
            .ThenByDescending(t => t.Achievement?.TotalGamerscore)
            .Take(_limit);

        _output.MostComplete(data);
    }

    public async Task SpentMostTimeWith()
    {
        var titles = await LoadTitles(false);

        var data = titles
            .OrderByDescending(t => t.Achievement?.ProgressPercentage)
            .ThenByDescending(t => t.Achievement?.TotalGamerscore)
            .ToDictionary(t => t, t => LoadStats(t).GetAwaiter().GetResult().FirstOrDefault(s => s.Name == "MinutesPlayed")?.IntValue ?? 0)
            .OrderByDescending(t => t.Value)
            .Take(_limit)
            .Select(kvp => new MinutesPlayed(kvp.Key.Name, kvp.Value));

        _output.SpentMostTimeWith(data);
    }

    public async Task Count()
    {
        var titles = await LoadTitles();
        var table = new Table();
        table.AddColumn("[bold]Profile[/]");
        table.AddColumn("[bold]Games[/]", c =>
        {
            c.Alignment = Justify.Right;
            var g = titles.GroupBy(t => t.Name);
            c.Footer($"{titles.Length}|{g.Count()}");

        });
        table.AddColumn("[bold]Achievements[/]", c => c.Alignment = Justify.Right);
        table.AddColumn("[bold]Gamerscore[/]", c => c.Alignment = Justify.Right);
        table.AddColumn("[bold]Hours played[/]", c => c.Alignment = Justify.Right);

        RenderProfile(table, titles.Where(t => t.IsLive).ToArray(), "Xbox Live", "green3_1");
        var x360 = titles.Where(t => !t.IsLive).ToArray();
        if (x360.Length > 0)
        {
            RenderProfile(table, x360, "Xbox 360", "cyan1");
            table.ShowFooters = true;
        }

        AnsiConsole.Write(table);
    }

    private static void RenderProfile(Table table, Title[] titles, string prefix, string color)
    {
        var profile = $"[{color}]{prefix}[/]";
        var c1 = $"[{color}]{titles.Length}[/]";
        var count = $"[{color}]{titles.Sum(t => t.Achievement?.CurrentAchievements)}[/]";
        var sum = $"[{color}]{titles.Sum(t => t.Achievement?.CurrentGamerscore)}[/]";
        var played = titles.Sum(t => LoadStats(t).GetAwaiter().GetResult().FirstOrDefault(s => s.Name == "MinutesPlayed")?.IntValue ?? 0);
        var hours = $"[{color}]{TimeSpan.FromMinutes(played).TotalHours:0.0}[/]";

        table.AddRow(profile, c1, count, sum, hours);
    }

    #endregion

    #region KQL

    public async Task<int> RunKustoQuery()
    {
        var context = new KustoQueryContext();
        var titles = await LoadTitles();
        switch (Settings.KustoQuerySource)
        {
            case "achievements":
                var tasks = titles
                    .OrderByDescending(t => t.Achievement.ProgressPercentage)
                    .Select(LoadAchievements);
                var data = await Task.WhenAll(tasks);
                var achievements = data.SelectMany(a => a);

                context.WrapDataIntoTable("achievements", achievements.Select(a => _mapper.Map<KqlAchievement>(a)).ToImmutableArray());

                break;
            case "stats":
                var tasks1 = titles
                    .OrderByDescending(t => t.Achievement.ProgressPercentage)
                    .Select(LoadStats);
                var data1 = await Task.WhenAll(tasks1);

                context.WrapDataIntoTable("stats", data1.SelectMany(s => s.Select(m => _mapper.Map<KqlMinutesPlayed>(m))).ToImmutableArray());

                break;
            default:

                context.WrapDataIntoTable("titles", titles.Select(a => _mapper.Map<KqlTitle>(a)).ToImmutableArray());

                break;
        }

        var kql = Settings.KustoQuery;
        if (kql.EndsWith(".kql", StringComparison.InvariantCultureIgnoreCase))
        {
            kql = await File.ReadAllTextAsync(Settings.KustoQuery);
        }
        var result = await context.RunQuery(kql);
        if (!string.IsNullOrEmpty(result.Error))
        {
            AnsiConsole.MarkupLineInterpolated($"[red]Error:[/] [silver]{result.Error}[/]");
            return 1;
        }

        _output.KustoQueryResult(result);
        return 0;
    }

    #endregion

    #region Infra

    public async Task GetLiveTitles()
    {
        var s = await _client.GetStringAsync("achievements/");
        await SaveJson(GetTitlesFilePath(Live), s);
    }

    public async Task<Title[]> LoadTitles(bool union = true)
    {
        var path = GetTitlesFilePath(Live);
        if (!File.Exists(path))
        {
            AnsiConsole.MarkupLine("[red]Error:[/] [silver]Data files cannot be found. Please run an update first[/]");
            return Array.Empty<Title>();
        }

        var a = await JsonHelper.FromFile<AchievementTitles>(path);
        _xuid = a.Xuid;
        IEnumerable<Title> titles = a.Titles.Select(t =>
        {
            t.IsLive = true;
            t.IsXbox360 = t.Devices.Contains("Xbox360");
            t.IsMobile = t.Devices.Contains("Mobile");
            t.HexId = ToHexId(t.TitleId);
            return t;
        }).OrderByDescending(t => t.Achievement.ProgressPercentage);

        path = GetTitlesFilePath(Xbox360);
        if (union && File.Exists(path))
        {
            a = await JsonHelper.FromFile<AchievementTitles>(path);
            titles = titles.Union(a.Titles);
        }

        return titles.ToArray();
    }

    public async Task GetAchievements(Title title)
    {
        var s = await _client.GetStringAsync("achievements/title/" + title.TitleId);
        await SaveJson(GetAchievementFilePath(title), s);
    }

    public async Task Get360Achievements(Title title)
    {
        var s = await _client.GetStringAsync($"achievements/x360/{_xuid}/title/{title.TitleId}");
        await SaveJson(GetAchievementFilePath(title), s);
    }

    public async Task<Achievement[]> LoadAchievements(Title title)
    {
        var path = GetAchievementFilePath(title);
        if (!File.Exists(path)) return Array.Empty<Achievement>();

        if (title.IsXbox360)
        {
            var details360 = await JsonHelper.FromFile<TitleDetails<Achievement>>(path);
            foreach (var a in details360.Achievements)
            {
                a.TitleName = title.Name;
            }
            return details360.Achievements;
        }

        var details = await JsonHelper.FromFile<TitleDetails<LiveAchievement>>(path);
        return _mapper.Map<Achievement[]>(details.Achievements);
    }

    public async Task GetStatsBulk(IEnumerable<Title> titles)
    {
        var changes = titles.Where(title =>
        {
            if (title.IsMobile || title.IsXbox360) return false;
            var x = new FileInfo(GetStatsFilePath(title));
            return title.TitleHistory.LastTimePlayed > x.LastWriteTimeUtc;
        }).ToArray();

        var a = await JsonHelper.FromFile<AchievementTitles>(GetTitlesFilePath(Live));
        var pages = changes.Chunk(100).Select(c => new PlayerStatsRequest
        {
            XUIDs = new[] { a.Xuid },
            Stats = c.Select(x => new Stat { Name = "MinutesPlayed", TitleId = x.TitleId }).ToArray()
        }).ToArray();

        AnsiConsole.MarkupInterpolated($"[white]Updating stats...[/] ");
        var cursor = Console.GetCursorPosition();
        AnsiConsole.Markup("[#f9f1a5]0%[/]");

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
                await SaveJson(GetStatsFilePath(Live, stat.TitleId), json);
            }
            Console.SetCursorPosition(cursor.Left, cursor.Top);
            AnsiConsole.MarkupInterpolated($"[#f9f1a5]{++i*100/pages.Length}%[/]");
        }
    }

    public static async Task<Stat[]> LoadStats(Title title)
    {
        var path = GetStatsFilePath(title);
        if (!File.Exists(path))
        {
            return Array.Empty<Stat>();
        }

        var details = await JsonHelper.FromFile<TitleStats>(path);
        return details.StatListsCollection.Length == 0 ? Array.Empty<Stat>() : details.StatListsCollection[0].Stats;
    }

    private static string GetTitlesFilePath(string env) => Path.Combine(DataFolder, $"titles.{env}.json");
    private static string GetAchievementFilePath(string env, string hexId) => Path.Combine(DataFolder, env, $"{hexId}\\achievements.json");
    private static string GetAchievementFilePath(Title title) => GetAchievementFilePath(title.IsLive ? Live : Xbox360, title.HexId);
    private static string GetStatsFilePath(string env, string hexId) => Path.Combine(DataFolder, env, $"{hexId}\\stats.json");
    private static string GetStatsFilePath(Title title) => GetStatsFilePath(title.IsLive ? Live : Xbox360, title.HexId);

    private static async Task SaveJson(string path, string json)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
        await File.WriteAllTextAsync(path, json);
    }

    private static string ToHexId(string titleId)
    {
        var id = uint.Parse(titleId);
        var bytes = BitConverter.GetBytes(id);
        bytes.SwapEndian(4);
        return bytes.ToHex();
    }

    #endregion
}