using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using System.Diagnostics;
using Xbl.Admin.Io;
using Xbl.Client;
using Xbl.Client.Models.Dbox;
using Xbl.Data;

namespace Xbl.Admin;

public sealed class App : AsyncCommand<Settings>
{
    private readonly IDboxClient _dbox;
    private readonly IXblClient _xbl;
    private readonly IDatabaseContext _dboxDb;

    public App(IDboxClient dbox, IXblClient xbl, [FromKeyedServices(DataSource.Dbox)] IDatabaseContext dboxDb)
    {
        _dbox = dbox;
        _xbl = xbl;
        _dboxDb = dboxDb;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        if (!Directory.Exists(DataSource.DataFolder)) Directory.CreateDirectory(DataSource.DataFolder);

        //1. get delta
        var newProducts = (await _dbox.GetDelta()).ToArray();

        //2. get Xbl game details
        var xblProducts = await _xbl.GetGameDetails(newProducts.SelectMany(p => p.Versions.Select(v => v.Value.ProductId)));

        //3. enrich delta
        var repo = await _dboxDb.GetRepository<Product>(DataTable.Store);
        foreach (var product in newProducts)
        {
            foreach (var (_, version) in product.Versions)
            {
                if (xblProducts.TryGetValue(version.ProductId, out var xblProduct))
                {
                    version.ReleaseDate = xblProduct.MarketProperties[0].OriginalReleaseDate;
                    if (xblProduct.MarketProperties.Length > 1) Debugger.Break();
                    product.Category = xblProduct.Properties.Category;
                }
                else
                {
                    Console.WriteLine($"Missing XBL product: {version.ProductId} {product.Title}");
                }
            }
        }

        await repo.BulkInsert(newProducts);
        return 0;
    }
}