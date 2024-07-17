using Utility.CommandLine;
using Xbl.Models;

namespace Xbl;

public class Program
{
    [Argument('h', "help", "Show this help")]
    private static bool Help { get; set; }

    [Argument('a', "api-key", "OpenXBL API Key for Xbox One and Series S|X profile information")]
    private static string ApiKey { get; set; }

    [Argument('p', "profile", "Your Xbox 360 Profile (STFS package) Path")]
    private static string ProfilePath { get; set; }

    [Argument('u', "update", "Update Xbox Live data files: all, achievements, stats (default=all)")]
    private static string Update { get; set; }

    [Argument('c', "count", "Quick summary about your loaded profiles")]
    private static bool Count { get; set; }

    [Argument('r', "rarest-achievements", "Rarest achievements")]
    private static bool Rarest { get; set; }

    [Argument('m', "most-complete-games", "Most complete games")]
    private static bool MostComplete { get; set; }

    [Argument('s', "spent-the-most-time-with", "Most played games")]
    private static bool MostPlayed { get; set; }

    [Argument('l', "limit", "Limit of the items displayed (default=50)")]
    private static int Limit { get; set; }

    [Argument('o', "output", "Output format: Console or JSON (default=Console)")]
    private static string Output { get; set; }

    private static async Task Main(string[] args)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("Xbox Achievement Stats v1.0");
        Console.WriteLine();
        Arguments.Populate();

        if (Help || args.Length == 0) ShowHelp();
        if (Limit == 0) Limit = 50;

        IOutput output = Output?.ToLower() switch
        {
            "json" => new XblJson(),
            _ => new XblConsole()
        };

        if (!string.IsNullOrWhiteSpace(ApiKey) && !Guid.TryParse(ApiKey, out _))
        {
            ShowError("Invalid API key");
            return;
        }

        var client = new XblClient(ApiKey, output);

        if (Update == string.Empty) Update = "all";
        if (Update is "all" or "achievements" or "stats") await client.Update(Update);

        var additionalTitles = LoadXboxProfileData();

        if (Count) await client.Count(additionalTitles);
        if (Rarest) await client.RarestAchievements(Limit);
        if (MostComplete) await client.MostComplete(Limit, additionalTitles);
        if (MostPlayed) await client.SpentMostTimeWith(Limit);

        Console.ForegroundColor = originalColor;
    }

    private static Title[] LoadXboxProfileData()
    {
        if (string.IsNullOrWhiteSpace(ProfilePath)) return null;

        Console.ForegroundColor = ConsoleColor.Gray;
        Console.Write("Extracting Xbox 360 profile...");

        if (!File.Exists(ProfilePath))
        {
            ShowError("Profile cannot be found");
            return null;
        }

        Title[] result = null;

        try
        {
            result = X360Profile.MapProfileToTitleArray(ProfilePath);
        }
        catch
        {
            ShowError("Profile cannot be loaded");
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("OK");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White;

        return result;
    }

    private static void ShowHelp()
    {
        Console.WriteLine("OPTIONS");
        var attributes = Arguments.GetArgumentInfo(typeof(Program)).ToArray();
        var options = attributes.ToDictionary(a =>
        {
            var value = a.Property.PropertyType != typeof(bool) ? $"=[arg]" : "";
            return $"-{a.ShortName}, --{a.LongName}{value}";
        }, a => a.HelpText);

        var maxLen = options.Keys.Max(k => k.Length);

        foreach (var (key,value) in options)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"  {key.PadRight(maxLen)}  ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(value);
        }
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("EXAMPLES");
        Console.WriteLine("  xbl -a={apiKey} -u");
        Console.WriteLine("  xbl -r");
        Console.WriteLine("  xbl -p=E00001D5D85ED487 -m");
        Console.WriteLine();
    }

    private static void ShowError(string error)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("Error: ");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine(error);
    }
}