using Xbl.Client.Models.Dbox;

namespace Xbl.Client.Repositories;

public interface IDboxRepository : IRepository
{
    Task<Dictionary<string, Product>> GetMarketplaceProducts();
    Task<Dictionary<string, Product>> GetStoreProducts();
    Task ConvertMarketplaceProducts();
    Task ConvertStoreProducts();
}