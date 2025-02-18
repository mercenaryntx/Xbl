using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Json;
using Xbl.Client.Models;
using Xbl.Xbox360.Models;

namespace Xbl.Client;

internal sealed class XblApp : AsyncCommand<XblSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, XblSettings settings)
    {
        if (settings.ExtendedHelp)
        {
            return PrintExtendedHelp();
        }

        if (!string.IsNullOrWhiteSpace(settings.ApiKey) && !Guid.TryParse(settings.ApiKey, out _))
        {
            return ShowError("Invalid API key");
        }

        var client = new XblClient(settings);

        var error = await Update(client);
        if (error < 0) return error;

        error = LoadXbox360Profile(client);
        if (error < 0) return error;

        if (!string.IsNullOrEmpty(settings.KustoQueryPath))
        {
            if (!File.Exists(settings.KustoQueryPath))
            {
                return ShowError("KQL file cannot be found");
            }

            return await client.RunKustoQuery();
        }

        switch (settings.Query)
        {
            case null:
                break;
            case "summary":
                await client.Count();
                break;
            case "rarity":
                await client.RarestAchievements();
                break;
            case "completeness":
                await client.MostComplete();
                break;
            case "time":
                await client.SpentMostTimeWith();
                break;
            case "weighted-rarity":
                await client.WeightedRarity();
                break;
            default:
                return ShowError("Unknown query alias");
        }

        return 0;
    }

    private static async Task<int> Update(XblClient client)
    {
        if (client.Settings.Update is not ("all" or "achievements" or "stats")) return 0;

        if (string.IsNullOrWhiteSpace(client.Settings.ApiKey))
        {
            return ShowError("API key is not set");
        }
        return await client.Update(client.Settings.Update);
    }

    private static int LoadXbox360Profile(XblClient client)
    {
        if (!string.IsNullOrWhiteSpace(client.Settings.ProfilePath))
        {
            var load = !string.IsNullOrEmpty(client.Settings.KustoQueryPath)
                ? client.Settings.KustoQuerySource ?? "titles"
                : client.Settings.Query is "summary" or "completeness"
                    ? "titles"
                    : "";

            int error;
            switch (load)
            {
                case "achievements":
                    error = LoadXboxProfileData(client.Settings, path => client.AdditionalAchievements = X360Profile.MapProfileToAchievementArray(path));
                    if (error < 0) return error;
                    break;
                case "titles":
                    error = LoadXboxProfileData(client.Settings, path => client.AdditionalTitles = X360Profile.MapProfileToTitleArray(path));
                    if (error < 0) return error;
                    break;
            }
        }

        return 0;
    }

    private static int LoadXboxProfileData(XblSettings settings, Action<string> action)
    {
        AnsiConsole.Markup("[silver]Extracting Xbox 360 profile... [/]");

        if (!File.Exists(settings.ProfilePath))
        {
            return ShowError("Profile cannot be found");
        }

        var sw = Stopwatch.StartNew();

        try
        {
            action(settings.ProfilePath);
        }
        catch
        {
            ShowError("Profile cannot be loaded");
        }

        AnsiConsole.MarkupLineInterpolated($"[#16c60c]OK[/] [silver]({sw.ElapsedMilliseconds}ms)[/]");
        Console.WriteLine();
     
        return 0;
    }

    private static int ShowError(string error)
    {
        AnsiConsole.MarkupLineInterpolated($"[red]Error:[/] [silver]{error}[/]");
        return -1;
    }

    private static int PrintExtendedHelp()
    {
        var titles = PrintJsonPanel(GetTitleExample(), "titles");
        var stats = PrintJsonPanel(GetStatExample(), "stats");
        var achievements = PrintJsonPanel(GetAchievementExample(), "achievements");
        AnsiConsole.Write(new Columns(titles, stats, achievements));
        return 0;
    }

    private static Panel PrintJsonPanel(object o, string header)
    {
        return new Panel(GetJsonText(o))
            .Header(header)
            .Collapse()
            .BorderColor(new Color(249, 251, 165));
    }

    private static KqlTitle GetTitleExample()
    {
        return new KqlTitle
        {
            TitleId = "1915865634",
            Name = "Lorem Ipsum: The Game",
            Devices = "PC|Xbox360|XboxOne|XboxSeries",
            CurrentAchievements = 18,
            TotalAchievements = 0,
            CurrentGamerscore = 560,
            TotalGamerscore = 1000,
            ProgressPercentage = 56,
            LastTimePlayed = DateTime.Parse("2025-02-02T15:54:38.4848326Z")
        };
    }

    private static KqlMinutesPlayed GetStatExample()
    {
        return new KqlMinutesPlayed
        {
            XUID = "2533274845176708",
            SCID = "40950100-bb3f-4769-906a-5eae3b67330a",
            TitleId = "996619018",
            Minutes = 341
        };
    }

    private static KqlAchievement GetAchievementExample()
    {
        return new KqlAchievement
        {
            Name = "Lorem ipsum",
            TitleId = "10027721",
            TitleName = "dolor sit amet",
            ProgressState = "Achieved",
            TimeUnlocked = DateTime.Parse("2016-09-11T12:25:10.6860000Z"),
            Platform = "Xbox360|XboxOne|XboxSeries",
            Description = "Lorem ipsum",
            LockedDescription = "dolor sit amet",
            Gamerscore = 100,
            IsRare = false,
            RarityPercentage = 12.34,
        };
    }

    private static JsonText GetJsonText(object o)
    {
        var green = new Color(74, 222, 128);
        var blue = new Color(14, 165, 233);
        var red = new Color(244, 63, 94);
        var yellow = new Color(249, 251, 165);

        return new JsonText(JsonSerializer.Serialize(o, new JsonSerializerOptions
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers =
                {
                    (JsonTypeInfo typeInfo) =>
                    {
                        foreach (var property in typeInfo.Properties)
                        {
                            var pi = property.AttributeProvider as PropertyInfo;
                            if (pi != null) property.Name = pi.Name;
                        }
                    }
                }
            }
        }))
        {
            BooleanStyle = green,
            NumberStyle = green,
            BracesStyle = Color.Silver,
            BracketsStyle = Color.Silver,
            MemberStyle = blue,
            ColonStyle = yellow,
            CommaStyle = Color.Silver,
            StringStyle = red,
            NullStyle = Color.Silver
        };

    }

}