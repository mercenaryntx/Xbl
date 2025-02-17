using System.Diagnostics;
using Spectre.Console;
using Spectre.Console.Cli;
using Xbl.Client.Models;

namespace Xbl.Client;

internal sealed class XblApp : AsyncCommand<XblSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, XblSettings settings)
    {
        //if (settings.Help || context.Arguments.Count == 0) ShowHelp();

        IOutput output = settings.Output?.ToLower() switch
        {
            "json" => new XblJson(),
            _ => new XblConsole()
        };

        if (!string.IsNullOrWhiteSpace(settings.ApiKey) && !Guid.TryParse(settings.ApiKey, out _))
        {
            return ShowError("Invalid API key");
        }

        var client = new XblClient(settings.ApiKey, output);

        var update = settings.Update;
        if (update == string.Empty) update = "all";
        if (update is "all" or "achievements" or "stats")
        {
            if (string.IsNullOrWhiteSpace(settings.ApiKey))
            {
                return ShowError("API key is not set");
            }
            await client.Update(update);
        }

        Title[] additionalTitles = null;
        if (!string.IsNullOrWhiteSpace(settings.ProfilePath))
        {
            var error = LoadXboxProfileData(settings, out additionalTitles);
            if (error < 0) return error;
        }

        if (settings.Count) await client.Count(additionalTitles);
        if (settings.Rarest) await client.RarestAchievements(settings.Limit);
        if (settings.MostComplete) await client.MostComplete(settings.Limit, additionalTitles);
        if (settings.MostPlayed) await client.SpentMostTimeWith(settings.Limit);
        if (settings.WeightedAchievements) await client.WeightedRarity(settings.Limit);

        return 0;
    }

    private static int LoadXboxProfileData(XblSettings settings, out Title[] result)
    {
        result = null;
        AnsiConsole.Markup("[silver]Extracting Xbox 360 profile... [/]");

        if (!File.Exists(settings.ProfilePath))
        {
            return ShowError("Profile cannot be found");
        }

        var sw = Stopwatch.StartNew();

        try
        {
            result = X360Profile.MapProfileToTitleArray(settings.ProfilePath);
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

}