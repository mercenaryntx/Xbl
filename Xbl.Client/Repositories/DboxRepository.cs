using AutoMapper;
using Xbl.Client.Models.Dbox;
using Xbl.Client.Io;

namespace Xbl.Client.Repositories;

public class DboxRepository : RepositoryBase, IDboxRepository
{
    private readonly IMapper _mapper;
    private readonly IConsole _console;

    public DboxRepository(IMapper mapper, IConsole console)
    {
        _mapper = mapper;
        _console = console;
    }

    public async Task<Dictionary<string, Product>> GetMarketplaceProducts()
    {
        var collections = await LoadCollections<Dictionary<string, Product>>(DataTable.Marketplace);
        return collections.Single();
    }

    public async Task<Dictionary<string, Product>> GetStoreProducts()
    {
        var collections = await LoadCollections<Dictionary<string, Product>>(DataTable.Store);
        return collections.Single();
    }

    public async Task ConvertMarketplaceProducts()
    {
        var collections = await LoadCollections<MarketplaceProductCollection>($"_{DataTable.Marketplace}");
        var grouping = collections.SelectMany(c => c.Products).GroupBy(p => p.TitleId).Select(g =>
        {
            var p = new Product { Versions = new Dictionary<string, ProductVersion> { { Device.Xbox360, new ProductVersion() }}};
            foreach (var i in g)
            {
                i.DefaultTitle = i.DefaultTitle.Replace("Full Game - ", "");
                _mapper.Map(i, p);
                _mapper.Map(i, p.Versions[Device.Xbox360]);
            }

            return p;
        }).ToDictionary(p => p.TitleId);
        await SaveJson(Path.Combine(DataSource.DataFolder, DataSource.Dbox, $"{DataTable.Marketplace}.{DateTime.Now.Ticks}.json"), grouping);
    }

    public async Task ConvertStoreProducts()
    {
        var storeItems = await LoadCollections<StoreProductCollection>($"_{DataTable.Store}");
        var grouping = storeItems
            .SelectMany(c => c.Products)
            .Where(p => !string.IsNullOrEmpty(p.TitleId) && p.ProductType == "Game")
            .GroupBy(p => p.TitleId)
            .ToDictionary(g => g.Key, _mapper.Map<Product>);
        await SaveJson(Path.Combine(DataSource.DataFolder, DataSource.Dbox, $"{DataTable.Store}.{DateTime.Now.Ticks}.json"), grouping);
    }

    private async Task<T[]> LoadCollections<T>(string prefix)
    {
        var tasks = Directory
            .GetFiles(Path.Combine(DataSource.DataFolder, DataSource.Dbox), $"{prefix}.*.json")
            .Select(LoadJson<T>);

        return await Task.WhenAll(tasks);
    }
}