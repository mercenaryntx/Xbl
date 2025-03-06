using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;
using Xbl.Client.Infrastructure;
using Xbl.Client.Io;
using Xbl.Client.Models.Dbox;
using Xbl.Client.Models.Xbl.Achievements;
using Xbl.Client.Models.Xbl.Marketplace;
using Xbl.Client.Models.Xbl.Player;
using Xbl.Client.Queries;
using Xbl.Client.Repositories;
using Xbl.Data;

namespace Xbl.Client;

public sealed class App : AsyncCommand<Settings>
{
    private readonly IConsole _console;
    private readonly IExtendedHelp _extendedHelp;
    private readonly IXblClient _xblClient;
    private readonly IXbox360ProfileImporter _importer;
    private readonly IKustoQueryExecutor _kustoQueryExecutor;
    private readonly IBuiltInQueries _builtInQueries;
    private readonly IServiceProvider _services;

    private Settings _settings;

    public App(IConsole console, IExtendedHelp extendedHelp, IXblClient xblClient, IXbox360ProfileImporter importer, IKustoQueryExecutor kustoQueryExecutor, IBuiltInQueries builtInQueries, IServiceProvider services)
    {
        _console = console;
        _extendedHelp = extendedHelp;
        _xblClient = xblClient;
        _importer = importer;
        _kustoQueryExecutor = kustoQueryExecutor;
        _builtInQueries = builtInQueries;
        _services = services;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            //await _console.Progress(async ctx =>
            //{
            //    await MigrateLiveDataToSqlite(ctx);
            //    //await MigrateXbox360DataToSqlite(ctx);
            //    //await MigrateDboxDataToSqlite(ctx);
            //    //await MigrateXblDataToSqlite(ctx);
            //});

            //await _dbox.ConvertStoreProducts();
            //await _dbox.ConvertMarketplaceProducts();
        }
        catch (Exception ex)
        {
            Debugger.Break();
        }

        _settings = settings;
        IOutput output = settings.Output?.ToLower() switch
        {
            "json" => new JsonOutput(),
            _ => _console
        };

        if (settings.ExtendedHelp)
        {
            return _extendedHelp.Print();
        }

        var error = await Update();
        if (error < 0) return error;

        error = await ImportXbox360Profile();
        if (error < 0) return error;

        var sw = Stopwatch.StartNew();
        if (!string.IsNullOrEmpty(settings.KustoQuery))
        {
            var result = await _kustoQueryExecutor.RunKustoQuery();
            if (!string.IsNullOrEmpty(result.Error))
            {
                return _console.ShowError(result.Error.EscapeMarkup());
            }
            output.KustoQueryResult(result);
            return 0;
        }

        switch (settings.Query)
        {
            case null:
                break;
            case "summary":
                output.Render(await _builtInQueries.Count());
                break;
            case "rarity":
                output.Render(await _builtInQueries.RarestAchievements());
                break;
            case "completeness":
                output.Render(await _builtInQueries.MostComplete());
                break;
            case "time":
                output.Render(await _builtInQueries.SpentMostTimeWith());
                break;
            case "weighted-rarity":
                output.Render(await _builtInQueries.WeightedRarity());
                break;
            case "categories":
                output.Render(await _builtInQueries.Categories());
                break;
            default:
                return _console.ShowError("Unknown query alias");
        }
        Console.WriteLine($"Query executed in {sw.ElapsedMilliseconds}ms");

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

    #region Migration

    private async Task MigrateLiveDataToSqlite(IProgressContext ctx)
    {
        var task1 = ctx.AddTask("[white]Migrating the titles[/]", maxValue: 2);
        var dbContext = _services.GetRequiredKeyedService<IDatabaseContext>(DataSource.Live);
        task1.Increment(1);

        var xbl = _services.GetRequiredService<IXblRepository>();
        var titleRepository = await dbContext.GetRepository<Title>();
        var titles = await xbl.LoadTitles(false);

        await titleRepository.BulkInsert(titles);
        task1.Increment(2);

        var task2 = ctx.AddTask("[white]Migrating the achievements[/]", maxValue: titles.Length);
        var task3 = ctx.AddTask("[white]Migrating the stats[/]", maxValue: titles.Length);

        var achievementRepository = await dbContext.GetRepository<Achievement>();
        var statsRepository = await dbContext.GetRepository<Stat>();

        foreach (var title in titles)
        {
            var a = await xbl.LoadAchievements(title);
            var i = 1;
            foreach (var achievement in a)
            {
                achievement.Id = i++;
            }
            await achievementRepository.BulkInsert(a);
            task2.Increment(1);
            await statsRepository.BulkInsert(await xbl.LoadStats(title));
            task3.Increment(1);
        }
    }

    private async Task MigrateXbox360DataToSqlite(IProgressContext ctx)
    {
        var task1 = ctx.AddTask("[white]Migrating the titles[/]", maxValue: 2);
        var dbContext = _services.GetRequiredKeyedService<IDatabaseContext>(DataSource.Xbox360);
        task1.Increment(1);

        var xbl = _services.GetRequiredService<IXblRepository>();
        var titleRepository = await dbContext.GetRepository<Title>();
        var titles = await xbl.LoadTitles();
        titles = titles.Where(t => t.Source == DataSource.Xbox360).ToArray();

        await titleRepository.BulkInsert(titles);
        task1.Increment(2);

        var task2 = ctx.AddTask("[white]Migrating the achievements[/]", maxValue: titles.Length);

        var achievementRepository = await dbContext.GetRepository<Achievement>();

        foreach (var title in titles)
        {
            var a = await xbl.LoadAchievements(title);
            var i = 1;
            foreach (var achievement in a)
            {
                achievement.Id = i++;
            }
            await achievementRepository.BulkInsert(a);
            task2.Increment(1);
        }
    }

    private async Task MigrateDboxDataToSqlite(IProgressContext ctx)
    {
        var task1 = ctx.AddTask("[white]Migrating the Dbox tables[/]", maxValue: 2);
        var dbContext = _services.GetRequiredKeyedService<IDatabaseContext>(DataSource.Dbox);

        var dbox = _services.GetRequiredService<IDboxRepository>();
        var marketplaceRepository = await dbContext.GetRepository<Product>(DataTable.Marketplace);
        var marketplaceProducts = await dbox.GetMarketplaceProducts();

        await marketplaceRepository.BulkInsert(marketplaceProducts.Values);
        task1.Increment(1);

        var storeRepository = await dbContext.GetRepository<Product>(DataTable.Store);
        var storeProducts = await dbox.GetStoreProducts();
        await storeRepository.BulkInsert(storeProducts.Values);
        task1.Increment(1);
    }

    private async Task MigrateXblDataToSqlite(IProgressContext ctx)
    {
        var dbContext = _services.GetRequiredKeyedService<IDatabaseContext>(DataSource.Xbl);

        var dbox = _services.GetRequiredService<IDboxRepository>();
        var tasks = Directory
            .GetFiles(Path.Combine(DataSource.DataFolder, DataSource.Xbl), "details.*.json")
            .Select(dbox.LoadJson<XblProductCollection>);

        var details = await Task.WhenAll(tasks);

        var task1 = ctx.AddTask("[white]Migrating the Xbl tables[/]", maxValue: details.Length);

        var repository = await dbContext.GetRepository<XblProduct>("product");

        foreach (var collection in details)
        {
            await repository.BulkInsert(collection.Products);
            task1.Increment(1);
        }
    }

    #endregion
}