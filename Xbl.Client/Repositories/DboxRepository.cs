using System.Diagnostics;
using AutoMapper;
using Xbl.Client.Models.Dbox;
using Xbl.Client.Models.Xbl.Marketplace;
using Product = Xbl.Client.Models.Dbox.Product;

namespace Xbl.Client.Repositories;

public class DboxRepository : RepositoryBase, IDboxRepository
{
    private readonly IMapper _mapper;

    public DboxRepository(IMapper mapper)
    {
        _mapper = mapper;
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
        var categories = await LoadJson<Category[]>(Path.Combine(DataSource.DataFolder, DataSource.Dbox, "categories.json"));
        var cat = categories.ToDictionary(c => c.Id);
        var collections = await LoadCollections<MarketplaceProductCollection>($"_{DataTable.Marketplace}");

        var grouping = collections.SelectMany(c => c.Products).GroupBy(p => p.TitleId).Select(g =>
        {
            var p = new Product { Versions = new Dictionary<string, ProductVersion> { { Device.Xbox360, new ProductVersion() }}};
            foreach (var i in g)
            {
                i.DefaultTitle = i.DefaultTitle.Replace("Full Game - ", "");
                _mapper.Map(i, p);
                _mapper.Map(i, p.Versions[Device.Xbox360]);
                if (cat.TryGetValue(i.Categories.Last(), out var category)) p.Category = category.Name;
            }

            return p;
        }).ToDictionary(p => p.TitleId);
        await SaveJson(Path.Combine(DataSource.DataFolder, DataSource.Dbox, $"{DataTable.Marketplace}.{DateTime.Now.Ticks}.json"), grouping);
    }

    public async Task ConvertStoreProducts()
    {
        var xblpc = await LoadCollections<XblProductCollection>("details", "xbl");
        var xblProducts = xblpc.SelectMany(c => c.Products).ToDictionary(p => p.ProductId);

        var storeItems = await LoadCollections<StoreProductCollection>($"_{DataTable.Store}");
        var grouping = storeItems
            .SelectMany(c => c.Products)
            .Where(p => !string.IsNullOrEmpty(p.TitleId) && p.ProductType == "Game")
            .GroupBy(p => p.TitleId)
            .ToDictionary(g => g.Key, p =>
            {
                var mapped = _mapper.Map<Product>(p);
                foreach (var (_, version) in mapped.Versions)
                {
                    if (xblProducts.TryGetValue(version.ProductId, out var xblProduct))
                    {
                        version.ReleaseDate = xblProduct.MarketProperties[0].OriginalReleaseDate;
                        if (xblProduct.MarketProperties.Length > 1) Debugger.Break();
                        mapped.Category = xblProduct.Properties.Category;
                    }
                    else
                    {
                        Console.WriteLine($"Missing XBL product: {version.ProductId} {mapped.Title}");
                    }
                }
                return mapped;
            });
        await SaveJson(Path.Combine(DataSource.DataFolder, DataSource.Dbox, $"{DataTable.Store}.{DateTime.Now.Ticks}.json"), grouping);
    }

    private async Task<T[]> LoadCollections<T>(string prefix, string folder = DataSource.Dbox)
    {
        var tasks = Directory
            .GetFiles(Path.Combine(DataSource.DataFolder, folder), $"{prefix}.*.json")
            .Select(LoadJson<T>);

        return await Task.WhenAll(tasks);
    }
}