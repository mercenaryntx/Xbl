using Xbl.Models;

namespace Xbl;

public class XblConsole : IOutput
{
    public void RarestAchievements(IEnumerable<RarestAchievementItem> data)
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

    public void MostComplete(IEnumerable<Title> data)
    {
        var i = 0;
        foreach (var title in data)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($"{++i:D3}. ");
            var x = title.Devices.FirstOrDefault(d => d.StartsWith("Xbox")) ?? "XboxOne";
            x = x.Replace("Xbox", "Xbox ");
            switch (x)
            {
                case "Xbox Series":
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    break;
                case "Xbox 360":
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
            }
            Console.Write($"[{x}] {title.Name} ");
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