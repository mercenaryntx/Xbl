using System.Diagnostics;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;
using Xbl.Client.Io;
using Xbl.Client.Queries;

namespace Xbl.Client;

public sealed class App : AsyncCommand<Settings>
{
    private readonly IConsole _console;
    private readonly IServiceProvider _services;

    private Settings _settings;

    public App(IConsole console, IServiceProvider services)
    {
        _console = console;
        _services = services;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        if (!Directory.Exists(DataSource.DataFolder)) Directory.CreateDirectory(DataSource.DataFolder);

        _settings = settings;
        IOutput output = settings.Output?.ToLower() switch
        {
            "json" => new JsonOutput(),
            _ => _console
        };

        if (settings.ExtendedHelp)
        {
            return _services.GetRequiredService<IExtendedHelp>().Print();
        }

        try
        {
            var code = await Update();
            if (code < 0) return code;

            code = await ImportXbox360Profile();
            if (code < 0) return code;

            return await RunKustoQuery(output) ?? await RunBuiltInQueries(output);
        }
        catch (SqliteException e) when (e.SqliteErrorCode == 14)
        {
            return _console.ShowError("Database not found. Please update first");
        }
        catch (Exception e)
        {
            Debugger.Break();
            throw;
        }
    }

    private async Task<int> Update()
    {
        if (_settings.Update is not ("all" or "achievements" or "stats")) return 0;
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            return _console.ShowError("API key is not set");
        }
        if (!Guid.TryParse(_settings.ApiKey, out _))
        {
            return _console.ShowError("Invalid API key");
        }
        return await _services.GetRequiredService<IXblClient>().Update();
    }

    private async Task<int> ImportXbox360Profile()
    {
        if (string.IsNullOrWhiteSpace(_settings.ProfilePath)) return 0;
        if (!File.Exists(_settings.ProfilePath))
        {
            return _console.ShowError("Profile cannot be found");
        }

        return await _services.GetRequiredService<IXbox360ProfileImporter>().Import();
    }

    private async Task<int?> RunKustoQuery(IOutput output)
    {
        if (string.IsNullOrEmpty(_settings.KustoQuery)) return null;

        var result = await _services.GetRequiredService<IKustoQueryExecutor>().RunKustoQuery();
        if (!string.IsNullOrEmpty(result.Error))
        {
            return _console.ShowError(result.Error.EscapeMarkup());
        }
        output.KustoQueryResult(result);
        return 0;
    }

    private async Task<int> RunBuiltInQueries(IOutput output)
    {
        var builtInQueries = _services.GetRequiredService<IBuiltInQueries>();
        switch (_settings.Query)
        {
            case null:
                break;
            case "summary":
                output.Render(await builtInQueries.Count());
                break;
            case "rarity":
                output.Render(await builtInQueries.RarestAchievements());
                break;
            case "completeness":
                output.Render(await builtInQueries.MostComplete());
                break;
            case "time":
                output.Render(await builtInQueries.SpentMostTimeWith());
                break;
            case "weighted-rarity":
                output.Render(await builtInQueries.WeightedRarity());
                break;
            case "categories":
                output.Render(await builtInQueries.Categories());
                break;
            default:
                return _console.ShowError("Unknown query alias");
        }

        return 0;
    }
}