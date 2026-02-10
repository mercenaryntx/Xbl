using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Xbl.Client;
using Xbl.Client.Models.Xbl.Marketplace;
using Xbl.Data;

namespace Xbl.Admin.Io;

public class XblClient : IXblClient
{
    private readonly IDatabaseContext _xbl;
    private readonly HttpClient _client;

    public XblClient(HttpClient client, [FromKeyedServices(DataSource.Xbl)] IDatabaseContext xbl)
    {
        _client = client;
        _xbl = xbl.Mandatory();
    }

    public async Task<Dictionary<string, XblProduct>> GetGameDetailsByProductId(IEnumerable<string> ids)
    {
        var result = new Dictionary<string, XblProduct>();
        var productRepository = await _xbl.GetRepository<XblProduct>("product");
        foreach (var id in ids.Chunk(100))
        {
            var response = await _client.PostAsJsonAsync("marketplace/details", new { products = string.Join(',', id) });
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var collection = JsonSerializer.Deserialize<XblProductCollection>(content);
            await productRepository.BulkInsert(collection.Products);
            foreach (var product in collection.Products)
            {
                result.Add(product.ProductId, product);
            }
        }

        return result;
    }

    public async Task<Dictionary<string, XblProduct>> GetGameDetailsByTitleId(string titleId)
    {
        var response = await _client.GetAsync("marketplace/title/" + titleId);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var collection = JsonSerializer.Deserialize<XblProductCollection>(content);
        return collection.Products.ToDictionary(p => p.Id);
    }
}