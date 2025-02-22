using System.Text.Json.Serialization;

namespace Xbl.Client.Models.Xbl;

public class Title
{
    [JsonPropertyName("titleId")]
    public string IntId { get; set; }
    [JsonPropertyName("hexId")]
    public string HexId { get; set; }
    [JsonPropertyName("pfn")]
    public string Pfn { get; set; }
    [JsonPropertyName("bingId")]
    public string BingId { get; set; }
    [JsonPropertyName("serviceConfigId")]
    public string ServiceConfigId { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("devices")]
    public string[] CompatibleDevices { get; set; }
    [JsonPropertyName("displayImage")]
    public string DisplayImage { get; set; }
    [JsonPropertyName("mediaItemType")]
    public string MediaItemType { get; set; }
    [JsonPropertyName("modernTitleId")]
    public string ModernTitleId { get; set; }
    [JsonPropertyName("isBundle")]
    public bool IsBundle { get; set; }
    [JsonPropertyName("achievement")]
    public AchievementSummary Achievement { get; set; }
    [JsonPropertyName("images")]
    public Image[] Images { get; set; }
    [JsonPropertyName("titleHistory")]
    public TitleHistory TitleHistory { get; set; }
    [JsonPropertyName("xboxLiveTier")]
    public string XboxLiveTier { get; set; }
    [JsonPropertyName("isStreamable")]
    public bool IsStreamable { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; }
    [JsonPropertyName("originalConsole")]
    public string OriginalConsole { get; set; }
    [JsonPropertyName("isBackCompat")]
    public bool IsBackCompat { get; set; }
    [JsonPropertyName("products")]
    public Dictionary<string, TitleProduct> Products { get; set; }
    [JsonPropertyName("category")]
    public string Category { get; set; }
}