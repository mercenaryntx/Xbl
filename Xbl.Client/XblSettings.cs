using System.ComponentModel;
using Spectre.Console.Cli;

namespace Xbl.Client;

public sealed class XblSettings : CommandSettings
{
    [CommandOption("-a|--api-key")]
    [Description("OpenXBL API Key for Xbox One and Series S|X profile information")]
    public string ApiKey { get; init; }

    [CommandOption("-c|--count")]
    [Description("Quick summary about your loaded profiles")]
    public bool Count { get; init; }

    //[CommandOption("-h|--help")]
    //[Description("Show this help")]
    //public bool Help { get; init; }

    [CommandOption("-l|--limit")]
    [Description("Limit of the items displayed (default=50)")]
    [DefaultValue(50)]
    public int Limit { get; init; }

    [CommandOption("-m|--most-complete-games")]
    [Description("Most complete games")]
    public bool MostComplete { get; init; }

    [CommandOption("-o|--output")]
    [Description("Output format: Console or JSON (default=Console)")]
    public string Output { get; init; }

    [CommandOption("-p|--profile")]
    [Description("Your Xbox 360 Profile (STFS package) Path")]
    public string ProfilePath { get; init; }

    [CommandOption("-r|--rarest-achievements")]
    [Description("Rarest achievements")]
    public bool Rarest { get; init; }

    [CommandOption("-s|--spent-the-most-time-with")]
    [Description("Most played games")]
    public bool MostPlayed { get; init; }

    [CommandOption("-u|--update")]
    [Description("Update Xbox Live data files: all, achievements, stats (default=all)")]
    public string Update { get; init; }

    [CommandOption("-w|--weighted-rarity")]
    [Description("Weighted rare achievements")]
    public bool WeightedAchievements { get; init; }

}