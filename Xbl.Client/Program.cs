using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;
using Xbl.Client.Infrastructure;
using Xbl.Client.Io;
using Xbl.Client.Queries;

namespace Xbl.Client;

public class Program
{
    private static async Task<int> Main(string[] args)
    {
        AnsiConsole.MarkupLine("[white]Xbox Achievement Stats v2.0[/]");
        Console.WriteLine();

        var app = new CommandApp<App>(ConfigureServices());
        app.Configure(c =>
        {
            c.SetApplicationName("xbl");
            c.AddExample("-a={apiKey} -u");
            c.AddExample("-q=rarity");
            c.AddExample("-q=completeness -l=1000 -o=json");
            c.AddExample("-s=achievements -k=stackedareachart.kql");
        });

        args = args.Length == 0 
            ? new[] {"-h"} 
            : args.Select(a => a == "-u" ? "-u=all" : a).ToArray();

        return await app.RunAsync(args);
    }

    private static XblTypeRegistrar ConfigureServices()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());

        var services = new ServiceCollection();
        services.AddSingleton<IConsole, ConsoleOutput>();
        services.AddSingleton<IJsonRepository, JsonRepository>();
        services.AddSingleton<IXblClient, XblClient>();
        services.AddSingleton<IXbox360ProfileImporter, Xbox360ProfileImporter>();
        services.AddSingleton<IBuiltInQueries, BuiltInQueries>();
        services.AddSingleton<IKustoQueryExecutor, KustoQueryExecutor>();
        services.AddSingleton<IExtendedHelp, ExtendedHelp>();
        services.AddSingleton(config.CreateMapper());

        return new XblTypeRegistrar(services);
    }
}