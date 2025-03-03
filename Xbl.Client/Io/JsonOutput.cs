using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using KustoLoco.Core;
using KustoLoco.Core.Settings;
using KustoLoco.Rendering;
using NotNullStrings;
using Spectre.Console;
using Xbl.Client.Models;

namespace Xbl.Client.Io;

[ExcludeFromCodeCoverage]
public class JsonOutput : IOutput
{
    public void Render(ProfilesSummary summary)
    {
        Write(summary.Profiles, nameof(summary));
    }

    public void Render(IEnumerable<RarestAchievementItem> rarest)
    {
        Write(rarest, "rarity");
    }

    public void Render(IEnumerable<WeightedAchievementItem> weightedRarity)
    {
        Write(weightedRarity, "weighted-rarity");
    }

    public void Render(IEnumerable<CompletenessItem> data)
    {
        Write(data, "completeness");
    }

    public void Render(IEnumerable<MinutesPlayed> minutesPlayed)
    {
        Write(minutesPlayed, "time");
    }

    public void Render(IEnumerable<CategorySlice> slices)
    {
        Write(slices, "categories");
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