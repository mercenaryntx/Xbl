using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using Xbl.Admin.Io;
using Xbl.Client;
using Xbl.Client.Infrastructure;
using Xbl.Data.Extensions;

namespace Xbl.Admin;

[ExcludeFromCodeCoverage]
public class Program
{
    private static async Task<int> Main(string[] args)
    {
        var app = new CommandApp<App>(ConfigureServices());
        return await app.RunAsync(args);
    }

    private static XblTypeRegistrar ConfigureServices()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());

        var services = new ServiceCollection();
        services.AddSingleton(config.CreateMapper())
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