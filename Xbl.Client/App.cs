using System.Diagnostics;
using Spectre.Console.Cli;
using Xbl.Client.Io;
using Xbl.Client.Queries;
using Xbl.Client.Repositories;

namespace Xbl.Client;

public sealed class App : AsyncCommand<Settings>
{
    private readonly IConsole _console;
    private readonly IExtendedHelp _extendedHelp;
    private readonly IXblClient _xblClient;
    private readonly IXbox360ProfileImporter _importer;
    private readonly IKustoQueryExecutor _kustoQueryExecutor;
    private readonly IBuiltInQueries _builtInQueries;
    private readonly IDboxRepository _dbox;

    private Settings _settings;

    public App(IConsole console, IExtendedHelp extendedHelp, IXblClient xblClient, IXbox360ProfileImporter importer, IKustoQueryExecutor kustoQueryExecutor, IBuiltInQueries builtInQueries, IDboxRepository dbox)
    {
        _console = console;
        _extendedHelp = extendedHelp;
        _xblClient = xblClient;
        _importer = importer;
        _kustoQueryExecutor = kustoQueryExecutor;
        _builtInQueries = builtInQueries;
        _dbox = dbox;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            //await _dbox.ConvertStoreProducts();
            //await _dbox.ConvertMarketplaceProducts();
        }
        catch (Exception ex)
        {
            Debugger.Break();
        }

        _settings = settings;

        if (settings.ExtendedHelp)
        {
            return _extendedHelp.Print();
        }

        var error = await Update();
        if (error < 0) return error;

        error = await ImportXbox360Profile();
        if (error < 0) return error;

        if (!string.IsNullOrEmpty(settings.KustoQuery))
        {
            return await _kustoQueryExecutor.RunKustoQuery();
        }

        switch (settings.Query)
        {
            case null:
                break;
            case "summary":
                await _builtInQueries.Count();
                break;
            case "rarity":
                await _builtInQueries.RarestAchievements();
                break;
            case "completeness":
                await _builtInQueries.MostComplete();
                break;
            case "time":
                await _builtInQueries.SpentMostTimeWith();
                break;
            case "weighted-rarity":
                await _builtInQueries.WeightedRarity();
                break;
            case "categories":
                await _builtInQueries.Categories();
                break;
            default:
                return _console.ShowError("Unknown query alias");
        }

        return 0;
    }

    private async Task<int> Update()
    {
        if (!string.IsNullOrWhiteSpace(_settings.ApiKey) && !Guid.TryParse(_settings.ApiKey, out _))
        {
            return _console.ShowError("Invalid API key");
        }

        if (_settings.Update is not ("all" or "achievements" or "stats")) return 0;

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            return _console.ShowError("API key is not set");
        }
        return await _xblClient.Update();
    }

    private async Task<int> ImportXbox360Profile()
    {
        if (string.IsNullOrWhiteSpace(_settings.ProfilePath)) return 0;
        if (!File.Exists(_settings.ProfilePath))
        {
            return _console.ShowError("Profile cannot be found");
        }

        return await _importer.Import();
    }
}