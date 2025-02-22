using System.Text.Json.Serialization;

namespace Xbl.Client.Models.Dbox;

public class StoreProductCollection : IProductCollection
{
    [JsonPropertyName("items")]
    public StoreProduct[] Products { get; set; }
    [JsonPropertyName("count")]
    public int Count { get; set; }
}