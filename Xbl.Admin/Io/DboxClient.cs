using System.Text.Json;
using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using Xbl.Client;
using Xbl.Client.Models.Dbox;
using Xbl.Data;

namespace Xbl.Admin.Io;

public class DboxClient : IDboxClient
{
    private readonly HttpClient _client;
    private readonly IDatabaseContext _dbox;
    private readonly IMapper _mapper;

    public DboxClient(HttpClient client, [FromKeyedServices(DataSource.Dbox)] IDatabaseContext dbox, IMapper mapper)
    {
        _client = client;
        _dbox = dbox.Mandatory(SqliteOpenMode.ReadWriteCreate);
        _mapper = mapper;
    }

    public async Task<int> GetBaseline()
    {
        await GetBaseline<MarketplaceProductCollection, MarketplaceProduct>(DataTable.Marketplace, "&product_type=1");
        await GetBaseline<MarketplaceProductCollection, MarketplaceProduct>(DataTable.Marketplace, "&product_type=14");
        await GetBaseline<StoreProductCollection, StoreProduct>(DataTable.Store);
        return 0;
    }

    private async Task GetBaseline<TC,TT>(string type, string ext = "") where TC : IProductCollection<TT>
    {
        var repo = await _dbox.GetRepository<Product>(type);
        var i = 0;
        var l = 10000;
        var c = int.MaxValue;
        while (i * l < c)
        {
            AnsiConsole.MarkupLine($"Downloading {type}... {i * 100 * l / c}%");
            var s = await _client.GetStringAsync($"{type}/products?limit={l}&offset={i * l}{ext}");
            var data = JsonSerializer.Deserialize<TC>(s);
            c = data.Count;
            var products = data.Products;
            await repo.BulkInsert(products.Select(p => _mapper.Map<Product>(p)));
            i++;
        }
    }

    public async Task<IEnumerable<Product>> GetDelta()
    {
        var type = DataTable.Store;
        var l = 10000;
        var lastUpdated = await _dbox.QuerySingle<DateTime>($"SELECT UpdatedOn FROM {type} ORDER BY UpdatedOn DESC");

        var s = await _client.GetStringAsync($"{type}/products?product_type=Game&order_by=revision_id&order_direction=desc&limit={l}&offset=0");
        var data = JsonSerializer.Deserialize<StoreProductCollection>(s);
        return data.Products.Where(p => p.RevisionId > lastUpdated).Select(_mapper.Map<Product>).ToArray();
    }
}