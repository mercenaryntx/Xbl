﻿using System.Net.Http.Json;
using System.Text.Json;
using Spectre.Console;
using Xbl.Client.Models;

namespace Xbl.Client;

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
            AnsiConsole.Markup("[white]Getting titles... [/]");
            await GetTitles();
            var titles = await LoadTitles();
            AnsiConsole.MarkupLineInterpolated($"[#16c60c]OK[/] [white][[{titles.Length}]][/]");

            if (update is "all" or "achievements")
            {
                await UpdateTitles(titles, title => $"{title.TitleId}.json", "achievements", GetAchievements);
            }
            if (update is "all" or "stats")
            {
                await GetStatsBulk(titles);
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine();
            AnsiConsole.MarkupLineInterpolated($"[red]Error:[/] [silver]OpenXBL API returned an error [/] [red]({(int?)ex.StatusCode}) {ex.StatusCode}[/]");
        }
    }

    private static async Task UpdateTitles(IEnumerable<Title> titles, Func<Title, string> dataFile, string type, Func<string, Task> updateLogic)
    {
        var changes = titles.Where(title =>
        {
            var x = new FileInfo(Path.Combine(DataFolder, dataFile(title)));
            return title.TitleHistory.LastTimePlayed > x.LastWriteTimeUtc;
        }).ToArray();

        AnsiConsole.MarkupLineInterpolated($"[white]Found [/] [cyan1]{changes.Length}[/] [white]{type} changes[/]");

        for (var i = 0; i < changes.Length; i++)
        {
            var title = changes[i];
            AnsiConsole.MarkupInterpolated($"[silver][[{i + 1:D3}/{changes.Length:D3}]][/] [white]Updating [/] [cyan1]{title.Name}[/][white]... [/]");
            await updateLogic(title.TitleId);
            AnsiConsole.MarkupLine("[#16c60c]OK[/]");
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

    public async Task WeightedRarity(int limit)
    {
        var titles = await LoadTitles();
        var data = titles
            .OrderByDescending(t => t.Achievement.ProgressPercentage)
            .ToDictionary(t => t, t => LoadAchievements(t.TitleId).GetAwaiter().GetResult());

        var mostRare = data.Select(t => new WeightedAchievementItem(
            t.Key.Name,
            t.Key.Achievement,
            t.Value.Count(),
            t.Value.Count(a => a.ProgressState == "Achieved"),
            t.Value.Count(a => a.ProgressState == "Achieved" && a.Rarity.CurrentCategory == "Rare"),
            t.Value.Where(a => a.ProgressState == "Achieved")
                .Sum(a =>
                {
                    double score = int.Parse(a.Rewards.FirstOrDefault(r => r.ValueType == "Int")?.Value ?? "0");
                    var weightFactor = Math.Exp((100 - a.Rarity.CurrentPercentage) / 5.0);
                    return score / t.Key.Achievement.TotalGamerscore * weightFactor;
                })
        )).OrderByDescending(a => a.Weight).Take(limit);

        _output.WeightedRarity(mostRare);
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
        var a = additionalTitles?.ToArray();
        var table = new Table();
        table.AddColumn("[bold]Profile[/]");
        table.AddColumn("[bold]Games[/]", c =>
        {
            c.Alignment = Justify.Right;
            if (a == null) return;
            var allTitles = titles.Concat(a).ToArray();
            var g = allTitles.GroupBy(t => t.Name);
            c.Footer($"{allTitles.Length}|{g.Count()}");

        });
        table.AddColumn("[bold]Achievements[/]", c => c.Alignment = Justify.Right);
        table.AddColumn("[bold]Gamerscore[/]", c => c.Alignment = Justify.Right);
        table.AddColumn("[bold]Hours played[/]", c => c.Alignment = Justify.Right);

        RenderProfile(table, titles, "Xbox Live", "green3_1");

        if (additionalTitles != null)
        {
            RenderProfile(table, a, "Xbox 360", "cyan1");
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
        var played = titles.Sum(t => LoadStats(t.TitleId).GetAwaiter().GetResult().FirstOrDefault(s => s.Name == "MinutesPlayed")?.IntValue ?? 0);
        var hours = $"[{color}]{TimeSpan.FromMinutes(played).TotalHours:0.0}[/]";

        table.AddRow(profile, c1, count, sum, hours);
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
            AnsiConsole.MarkupLine("[red]Error:[/] [silver]Data files cannot be found. Please run an update first[/]");
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

        AnsiConsole.MarkupLineInterpolated($"[white]Updating stats[/] ");
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