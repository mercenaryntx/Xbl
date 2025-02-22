using System.Text.Json.Serialization;

namespace Xbl.Client.Models.Dbox;

public class MarketplaceProduct
{
    [JsonPropertyName("product_id")]
    public string ProductId { get; set; }
    [JsonPropertyName("product_type")]
    public int ProductType { get; set; }
    [JsonPropertyName("title_id")]
    public string TitleId { get; set; }
    [JsonPropertyName("effective_title_id")]
    public string EffectiveTitleId { get; set; }
    [JsonPropertyName("visibility_date")]
    public DateTime? VisibilityDate { get; set; }
    [JsonPropertyName("global_original_release_date")]
    public DateTime? GlobalOriginalReleaseDate { get; set; }
    [JsonPropertyName("developer_name")]
    public string DeveloperName { get; set; }
    [JsonPropertyName("publisher_name")]
    public string PublisherName { get; set; }
    [JsonPropertyName("categories")]
    public int[] Categories { get; set; }
    [JsonPropertyName("default_title")]
    public string DefaultTitle { get; set; }
}