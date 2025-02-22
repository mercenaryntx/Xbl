using System.Text.Json.Serialization;

namespace Xbl.Client.Models.Xbl;

public class Achievement
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("titleId")]
    public int TitleId { get; set; }
    [JsonPropertyName("titleName")]
    public string TitleName { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("sequence")]
    public int Sequence { get; set; }
    [JsonPropertyName("flags")]
    public int Flags { get; set; }
    [JsonPropertyName("unlockedOnline")]
    public bool UnlockedOnline { get; set; }
    [JsonPropertyName("unlocked")]
    public bool Unlocked { get; set; }
    [JsonPropertyName("isSecret")]
    public bool IsSecret { get; set; }
    [JsonPropertyName("platform")]
    [JsonConverter(typeof(PlatformConverter))]
    public string Platform { get; set; }
    [JsonPropertyName("gamerscore")]
    public int Gamerscore { get; set; }
    [JsonPropertyName("imageId")]
    public int ImageId { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }
    [JsonPropertyName("lockedDescription")]
    public string LockedDescription { get; set; }
    [JsonPropertyName("type")]
    public int Type { get; set; }
    [JsonPropertyName("isRevoked")]
    public bool IsRevoked { get; set; }
    [JsonPropertyName("timeUnlocked")]
    public DateTime TimeUnlocked { get; set; }
    [JsonPropertyName("rarity")]
    public Rarity Rarity { get; set; }
}