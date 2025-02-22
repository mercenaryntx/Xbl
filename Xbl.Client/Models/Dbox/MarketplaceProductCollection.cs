using System.Text.Json.Serialization;

namespace Xbl.Client.Models.Dbox;

public class MarketplaceProductCollection : IProductCollection
{
    [JsonPropertyName("items")]
    public MarketplaceProduct[] Products { get; set; }
    [JsonPropertyName("count")]
    public int Count { get; set; }
}

public interface IProductCollection
{
    public int Count { get; set; }
}