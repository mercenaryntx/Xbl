using System.Text.Json;
using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Xbl.Client;
using Xbl.Client.Io;
using Xbl.Client.Models.Dbox;
using Xbl.Data;

namespace Xbl.Admin.Io;

public class DboxClient : IDboxClient
{
    private readonly HttpClient _client;
    private readonly IDatabaseContext _dbox;
    private readonly IMapper _mapper;
    private readonly IConsole _console;

    public DboxClient(HttpClient client, [FromKeyedServices(DataSource.Dbox)] IDatabaseContext dbox, IMapper mapper, IConsole console)
    {
        _client = client;
        _dbox = dbox.Mandatory(SqliteOpenMode.ReadWriteCreate);
        _mapper = mapper;
        _console = console;
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
            _console.MarkupLine($"Downloading {type}... {i * 100 * l / c}%");
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
        throw new NotImplementedException();
    }
}