using Xbl.Client.Models.Dbox;
using Xbl.Client.Repositories;

namespace Xbl.Client.Io;

public class DboxClient : IDboxClient
{
    private readonly HttpClient _client;
    private readonly IDboxRepository _repository;
    private readonly IConsole _console;

    public DboxClient(HttpClient client, IDboxRepository repository, IConsole console)
    {
        _client = client;
        _repository = repository;
        _console = console;
    }

    public async Task<int> Update()
    {
        string MarketplaceFilename(int i) => $"_marketplace.{i}.json";
        await Update<MarketplaceProductCollection>("marketplace", _ => MarketplaceFilename(1), "&product_type=1");
        await Update<MarketplaceProductCollection>("marketplace", _ => MarketplaceFilename(14), "&product_type=14");
        await Update<StoreProductCollection>("store", i => $"_store.{i:D3}.json");
        return 0;
    }

    private async Task Update<T>(string type, Func<int, string> filename, string ext = "") where T : IProductCollection
    {
        var i = 0;
        var l = 10000;
        var c = int.MaxValue;
        while (i * l < c)
        {
            _console.MarkupLine($"Downloading {type}... {i * 100 * l / c}%");
            var s = await _client.GetStringAsync($"{type}/products?limit={l}&offset={i * l}{ext}");
            var path = Path.Combine(DataSource.DataFolder, DataSource.Dbox, filename(i));
            await _repository.SaveJson(path, s);
            if (c == int.MaxValue)
            {
                var collection = await _repository.LoadJson<T>(path);
                c = collection.Count;
            }
            i++;
        }
    }
}