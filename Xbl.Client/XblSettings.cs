using System.ComponentModel;
using Spectre.Console.Cli;

namespace Xbl.Client;

public sealed class XblSettings : CommandSettings
{
    [CommandOption("-a|--api-key")]
    [Description("OpenXBL API Key for Xbox One and Series S|X profile information")]
    public string ApiKey { get; init; }

    [CommandOption("-e")]
    [Description("Prints extended help information about the query sources")]
    public bool ExtendedHelp { get; init; }

    [CommandOption("-k|--kql")]
    [Description("Your Kustom query or *.kql file path")]
    public string KustoQuery { get; set; }

    [CommandOption("-l|--limit")]
    [Description("Limit of the items displayed [grey](default=50)[/]")]
    public int Limit { get; set; }

    [CommandOption("-o|--output")]
    [Description("Output format:\n  [#f9f1a5]console[/] [grey](default)[/]\n  [#f9f1a5]json[/]")]
    public string Output { get; init; }

    [CommandOption("-p|--profile")]
    [Description("Your Xbox 360 Profile (STFS package) path")]
    public string ProfilePath { get; init; }

    [CommandOption("-q|--query")]
    [Description("Select one of the built-in queries: \n  [#f9f1a5]summary[/] - Quick summary about your loaded profiles [grey](default)[/]\n  [#f9f1a5]completeness[/] - Your most complete games\n  [#f9f1a5]time[/] - Games you spent the most time with\n  [#f9f1a5]rarity[/] - Your rarest achievements\n  [#f9f1a5]weighted-rarity[/] - Your games with the most rarest achievements")]
    public string Query { get; init; }

    [CommandOption("-s|--source")]
    [Description("The source of you custom KQL query:\n  [#f9f1a5]titles[/] [grey](default)[/]\n  [#f9f1a5]achievements[/]\n  [#f9f1a5]stats[/]")]
    public string KustoQuerySource { get; init; }

    [CommandOption("-u|--update")]
    [Description("Update Xbox Live data files:\n  [#f9f1a5]all[/] [grey](default)[/]\n  [#f9f1a5]achievements[/]\n  [#f9f1a5]stats[/]")]
    public string Update { get; init; }
}