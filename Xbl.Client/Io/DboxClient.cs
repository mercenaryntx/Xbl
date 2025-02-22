using System.Net.Http.Headers;
using Xbl.Client.Models.Dbox;
using Xbl.Client.Repositories;

namespace Xbl.Client.Io;

public class DboxClient : IDboxClient
{
    private readonly Settings _settings;
    private readonly IDboxRepository _repository;
    private readonly IConsole _console;
    private readonly HttpClient _client = new();

    public DboxClient(Settings settings, IDboxRepository repository, IConsole console)
    {
        _settings = settings;
        _repository = repository;
        _console = console;
        _client.BaseAddress = new Uri("https://dbox.tools/api/");
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
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