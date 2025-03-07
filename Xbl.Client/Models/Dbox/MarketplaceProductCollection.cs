using System.Text.Json.Serialization;

namespace Xbl.Client.Models.Dbox;

public class MarketplaceProductCollection : IProductCollection<MarketplaceProduct>
{
    [JsonPropertyName("items")]
    public MarketplaceProduct[] Products { get; set; }
    [JsonPropertyName("count")]
    public int Count { get; set; }
}