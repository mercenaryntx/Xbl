using Spectre.Console.Cli;
using Xbl.Client.Io;
using Xbl.Client.Queries;

namespace Xbl.Client;

public sealed class App : AsyncCommand<Settings>
{
    private readonly IExtendedHelp _extendedHelp;
    private readonly IXblClient _client;
    private readonly IXbox360ProfileImporter _importer;
    private readonly IKustoQueryExecutor _kustoQueryExecutor;
    private readonly IBuiltInQueries _builtInQueries;
    private readonly IConsole _console;

    private Settings _settings;

    public App(IConsole console, IExtendedHelp extendedHelp, IXblClient client, IXbox360ProfileImporter importer, IKustoQueryExecutor kustoQueryExecutor, IBuiltInQueries builtInQueries)
    {
        _console = console;
        _extendedHelp = extendedHelp;
        _client = client;
        _importer = importer;
        _kustoQueryExecutor = kustoQueryExecutor;
        _builtInQueries = builtInQueries;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        _settings = settings;

        if (settings.ExtendedHelp)
        {
            return _extendedHelp.Print();
        }

        if (!string.IsNullOrWhiteSpace(settings.ApiKey) && !Guid.TryParse(settings.ApiKey, out _))
        {
            return _console.ShowError("Invalid API key");
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
            default:
                return _console.ShowError("Unknown query alias");
        }

        return 0;
    }

    private async Task<int> Update()
    {
        if (_settings.Update is not ("all" or "achievements" or "stats")) return 0;

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            return _console.ShowError("API key is not set");
        }
        return await _client.Update();
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

//if (settings.KustoQuery.EndsWith(".kql", StringComparison.InvariantCultureIgnoreCase) && !File.Exists(settings.KustoQuery))
//{
//    return _console.ShowError("KQL file cannot be found");
//}
