using System.ComponentModel;
using Spectre.Console.Cli;

namespace Xbl.Admin;

public sealed class Settings : CommandSettings
{
    [CommandOption("-a|--api-key")]
    [Description("OpenXBL API Key for Xbox One and Series S|X profile information")]
    public string ApiKey { get; init; }
}