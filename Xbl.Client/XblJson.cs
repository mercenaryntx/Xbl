using System.Text.Json;
using Xbl.Client.Models;

namespace Xbl.Client;

public class XblJson : IOutput
{
    public void RarestAchievements(IEnumerable<Records> rarest)
    {
        Write(rarest, nameof(rarest));
    }

    public void WeightedRarity(IEnumerable<WeightedAchievementItem> weightedRarity)
    {
        Write(weightedRarity, nameof(weightedRarity));
    }

    public void MostComplete(IEnumerable<Title> mostComplete)
    {
        Write(mostComplete, nameof(mostComplete));
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
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write("Output file ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(fileName);
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(" created.");
    }
}