using Xbl.Client.Models;

namespace Xbl.Client;

public class XblConsole : IOutput
{
    public void RarestAchievements(IEnumerable<Records> data)
    {
        var i = 0;
        foreach (var (title, achievement, currentPercentage) in data)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($"{++i:D3}. ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{title} ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($"{achievement} ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"({currentPercentage:F}%)");
        }
    }

    public void WeightedRarity(IEnumerable<WeightedAchievementItem> weightedRarity)
    {
        var i = 0;
        foreach (var (title, summary, totalCount, achievedCount, rareCount, weight) in weightedRarity)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($"{++i:D3}. ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{title} ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($"{summary.CurrentGamerscore}/{summary.TotalGamerscore} {rareCount}/{achievedCount}/{totalCount} ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"({weight:F})");
        }
    }

    public void MostComplete(IEnumerable<Title> data)
    {
        var i = 0;
        foreach (var title in data)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($"{++i:D3}. ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{title.Name} ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($"{title.Achievement.CurrentGamerscore}/{title.Achievement.TotalGamerscore} ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"({title.Achievement.ProgressPercentage:F}%)");
        }

    }

    public void SpentMostTimeWith(IEnumerable<MinutesPlayed> data)
    {
        var i = 0;
        foreach (var (title, played) in data)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($"{++i:D3}. ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{title} ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"{TimeSpan.FromMinutes(played).TotalHours:#.0}h");
        }
    }
}