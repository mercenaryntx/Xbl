using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Xbl.Client.Infrastructure;
using Xbl.Client.Io;
using Xbl.Client.Queries;
using Xbl.Client.Repositories;
using Xbl.Data.Extensions;

namespace Xbl.Client;

[ExcludeFromCodeCoverage]
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
            ? ["-h"]
            : args.Select(a => a == "-u" ? "-u=all" : a).ToArray();

        return await app.RunAsync(args);
    }

    private static XblTypeRegistrar ConfigureServices()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());

        var services = new ServiceCollection();
        services.AddSingleton<IConsole, ConsoleOutput>()
            .AddSingleton<IXblRepository, XblRepository>()
            .AddSingleton<IDboxRepository, DboxRepository>()
            .AddSingleton<IXbox360ProfileImporter, Xbox360ProfileImporter>()
            .AddSingleton<IBuiltInQueries, BuiltInQueries>()
            .AddSingleton<IKustoQueryExecutor, KustoQueryExecutor>()
            .AddSingleton<IExtendedHelp, ExtendedHelp>()
            .AddSingleton(config.CreateMapper())
            .AddData(DataSource.Live, DataSource.Xbox360, DataSource.Dbox, DataSource.Xbl);

        services.AddHttpClient<IXblClient, XblClient>((s, c) =>
                {
                    var settings = s.GetRequiredService<Settings>();
                    c.DefaultRequestHeaders.Add("x-authorization", settings.ApiKey);
                    c.BaseAddress = new Uri("https://xbl.io/api/v2/");
                });

        services.AddHttpClient<IDboxClient, DboxClient>(c =>
                {
                    c.BaseAddress = new Uri("https://dbox.tools/api/");
                    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                });

        return new XblTypeRegistrar(services);
    }
}