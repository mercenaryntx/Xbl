using Spectre.Console;
using Xbl.Client.Io;
using Xbl.Client.Models.Xbl;
using Xbl.Client.Repositories;

namespace Xbl.Client.Queries;

public class BuiltInQueries : IBuiltInQueries
{
    private readonly Settings _settings;
    private readonly IXblRepository _repository;
    private readonly IConsole _console;
    private readonly IOutput _output;

    public BuiltInQueries(Settings settings, IXblRepository repository, IConsole console)
    {
        _settings = settings;
        _repository = repository;
        _console = console;
        _output = settings.Output?.ToLower() switch
        {
            "json" => new JsonOutput(),
            _ => _console
        };
        _settings.Limit = _settings.Limit > 0 ? _settings.Limit : 50;
    }

    public async Task Count()
    {
        var titles = await _repository.LoadTitles();
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

        RenderProfile(table, titles.Where(t => t.Source == DataSource.Live).ToArray(), "Xbox Live", "green3_1");
        var x360 = titles.Where(t => t.Source == DataSource.Xbox360).ToArray();
        if (x360.Length > 0)
        {
            RenderProfile(table, x360, "Xbox 360", "cyan1");
            table.ShowFooters = true;
        }

        _console.Write(table);
    }

    private void RenderProfile(Table table, Title[] titles, string prefix, string color)
    {
        var profile = $"[{color}]{prefix}[/]";
        var c1 = $"[{color}]{titles.Length}[/]";
        var count = $"[{color}]{titles.Sum(t => t.Achievement?.CurrentAchievements)}[/]";
        var sum = $"[{color}]{titles.Sum(t => t.Achievement?.CurrentGamerscore)}[/]";
        var played = titles.Sum(t => _repository.LoadStats(t).GetAwaiter().GetResult().FirstOrDefault(s => s.Name == "MinutesPlayed")?.IntValue ?? 0);
        var hours = $"[{color}]{TimeSpan.FromMinutes(played).TotalHours:0.0}[/]";

        table.AddRow(profile, c1, count, sum, hours);
    }

    public async Task RarestAchievements()
    {
        var titles = await _repository.LoadTitles(false);
        var data = titles
            .OrderByDescending(t => t.Achievement.ProgressPercentage)
            .ToDictionary(t => t, t => _repository.LoadAchievements(t).GetAwaiter().GetResult());

        var rarest = data.SelectMany(kvp => kvp.Value.Where(a => a.Unlocked).Select(a => new RarestAchievementItem(
            kvp.Key.Name,
            a.Name,
            a.Rarity.CurrentPercentage
        ))).OrderBy(a => a.Percentage).Take(_settings.Limit);

        _output.RarestAchievements(rarest);
    }

    public async Task MostComplete()
    {
        var titles = await _repository.LoadTitles();
        var data = titles
            .OrderByDescending(t => t.Achievement?.ProgressPercentage)
            .ThenByDescending(t => t.Achievement?.TotalGamerscore)
            .Take(_settings.Limit);

        _output.MostComplete(data);
    }

    public async Task SpentMostTimeWith()
    {
        var titles = await _repository.LoadTitles(false);

        var data = titles
            .OrderByDescending(t => t.Achievement?.ProgressPercentage)
            .ThenByDescending(t => t.Achievement?.TotalGamerscore)
            .ToDictionary(t => t, t => _repository.LoadStats(t).GetAwaiter().GetResult().FirstOrDefault(s => s.Name == "MinutesPlayed")?.IntValue ?? 0)
            .OrderByDescending(t => t.Value)
            .Take(_settings.Limit)
            .Select(kvp => new MinutesPlayed(kvp.Key.Name, kvp.Value));

        _output.SpentMostTimeWith(data);
    }

    public async Task WeightedRarity()
    {
        var titles = await _repository.LoadTitles(false);
        var data = titles
            .OrderByDescending(t => t.Achievement.ProgressPercentage)
            .ToDictionary(t => t, t => _repository.LoadAchievements(t).GetAwaiter().GetResult());

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
        )).OrderByDescending(a => a.Weight).Take(_settings.Limit);

        _output.WeightedRarity(mostRare);
    }
}