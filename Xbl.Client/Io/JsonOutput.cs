using System.Text.Json;
using KustoLoco.Core;
using KustoLoco.Core.Settings;
using KustoLoco.Rendering;
using NotNullStrings;
using Spectre.Console;
using Xbl.Client.Models;

namespace Xbl.Client.Io;

public class JsonOutput : IOutput
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
        var ticks = DateTime.Now.Ticks;
        Write(json, $"kustom-{ticks}.json");

        if (result.Visualization.ChartType.IsBlank())
            return;

        var html = new KustoResultRenderer(new KustoSettingsProvider()).RenderToHtml(result);
        var fileName = $"kustom-{ticks}.html";
        File.WriteAllText(fileName, html);
        AnsiConsole.MarkupLineInterpolated($"[silver]Chart file[/] [#16c60c]{fileName}[/] [silver]created.[/]");
    }

    private static void Write<T>(IEnumerable<T> data, string prefix)
    {
        var json = JsonSerializer.Serialize(data);
        var fileName = $"{prefix}-{DateTime.Now.Ticks}.json";
        Write(json, fileName);
    }

    private static void Write(string json, string fileName)
    {
        File.WriteAllText(fileName, json);
        AnsiConsole.MarkupLineInterpolated($"[silver]Output file[/] [#16c60c]{fileName}[/] [silver]created.[/]");
    }
}