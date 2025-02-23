using System.Text.Json.Serialization;

namespace Xbl.Client.Models.Dbox;

public class Category
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("parent")]
    public int? Parent { get; set; }
    [JsonPropertyName("marketplace_id")]
    public int MarketplaceId { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("lft")]
    public int Left { get; set; }
    [JsonPropertyName("rght")]
    public int Right { get; set; }
    [JsonPropertyName("tree_id")]
    public int TreeId { get; set; }
    [JsonPropertyName("level")]
    public int Level { get; set; }
}