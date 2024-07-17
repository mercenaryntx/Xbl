using System.Text.Json;
using Xbl.Models;

namespace Xbl;

public class XblJson : IOutput
{
    public void RarestAchievements(IEnumerable<RarestAchievementItem> rarest)
    {
        Write(rarest, nameof(rarest));
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