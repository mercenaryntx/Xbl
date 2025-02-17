using System.Text.Json;
using Spectre.Console;
using Xbl.Client.Models;

namespace Xbl.Client;

public class XblJson : IOutput
{
    public void RarestAchievements(IEnumerable<RarestAchievementItem> rarest)
    {
        Write(rarest, nameof(rarest));
    }

    public void WeightedRarity(IEnumerable<WeightedAchievementItem> weightedRarity)
    {
        Write(weightedRarity, nameof(weightedRarity));
    }

    public void MostComplete(IEnumerable<Title> mostComplete)
    {
        Write(mostComplete.Select(m => new { m.Name, m.Achievement?.CurrentGamerscore, m.Achievement?.TotalGamerscore, m.Achievement?.ProgressPercentage }), nameof(mostComplete));
    }

    public void SpentMostTimeWith(IEnumerable<MinutesPlayed> minutesPlayed)
    {
        Write(minutesPlayed, nameof(minutesPlayed));
    }

    private static void Write<T>(IEnumerable<T> data, string prefix)
    {
        var json = JsonSerializer.Serialize(data);
        var fileName = $"{prefix}-{DateTime.Now.Ticks}.json";
        File.WriteAllText(fileName, json);
        AnsiConsole.MarkupLineInterpolated($"[silver]Output file[/] [#16c60c]{fileName}[/] [silver]created.[/]");
    }
}