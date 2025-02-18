using System.Text.Json;
using KustoLoco.Core;
using Spectre.Console;
using Xbl.Client.Models;

namespace Xbl.Client;

public class XblJson : IOutput
{
    public void RarestAchievements(IEnumerable<RarestAchievementItem> rarest)
    {
        Write(rarest, "rarity");
    }

    public void WeightedRarity(IEnumerable<WeightedAchievementItem> weightedRarity)
    {
        Write(weightedRarity, "weighted-rarity");
    }

    public void MostComplete(IEnumerable<Title> mostComplete)
    {
        Write(mostComplete.Select(m => new { m.Name, m.Achievement?.CurrentGamerscore, m.Achievement?.TotalGamerscore, m.Achievement?.ProgressPercentage }), "completeness");
    }

    public void SpentMostTimeWith(IEnumerable<MinutesPlayed> minutesPlayed)
    {
        Write(minutesPlayed, "time");
    }

    public void KustoQueryResult(KustoQueryResult result)
    {
        var json = result.ToJsonString();
        Write(json, "kustom");
    }

    private static void Write<T>(IEnumerable<T> data, string prefix)
    {
        var json = JsonSerializer.Serialize(data);
        Write(json, prefix);
    }

    private static void Write(string json, string prefix)
    {
        var fileName = $"{prefix}-{DateTime.Now.Ticks}.json";
        File.WriteAllText(fileName, json);
        AnsiConsole.MarkupLineInterpolated($"[silver]Output file[/] [#16c60c]{fileName}[/] [silver]created.[/]");
    }
}