using Xbl.Client.Models.Dbox;
using Xbl.Client.Repositories;

namespace Xbl.Client.Io;

public class DboxClient : IDboxClient
{
    private readonly Settings _settings;
    private readonly HttpClient _client;
    private readonly IDboxRepository _repository;
    private readonly IConsole _console;

    public DboxClient(Settings settings, HttpClient client, IDboxRepository repository, IConsole console)
    {
        _settings = settings;
        _client = client;
        _repository = repository;
        _console = console;
    }

    public async Task<int> Update()
    {
        await Update<MarketplaceProductCollection>("marketplace");
        await Update<StoreProductCollection>("store");
        return 0;
    }

    private async Task<int> Update<T>(string type, int i = 0) where T : IProductCollection
    {
        var pt = 1;
        var l = 10000;
        var c = int.MaxValue;
        while (i * l < c)
        {
            _console.MarkupLine($"Downloading {type}... {i * 100 * l / c}%");
            var s = await _client.GetStringAsync($"{type}/products?product_type={pt}&limit={l}&offset={i * l}");
            var path = Path.Combine(DataSource.DataFolder, DataSource.Dbox, $"_{type}.{pt}.{i:D5}.json");
            await _repository.SaveJson(path, s);
            if (c == int.MaxValue)
            {
                var collection = await _repository.LoadJson<T>(path);
                c = collection.Count;
            }
            i++;
        }

        return 0;
    }
}