using System.Text.Json.Serialization;

namespace Xbl.Client.Models;

public class Achievement
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("serviceConfigId")]
    public string ServiceConfigId { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("titleAssociations")]
    public TitleAssociation[] TitleAssociations { get; set; }
    [JsonPropertyName("progressState")]
    public string ProgressState { get; set; }
    [JsonPropertyName("progression")]
    public Progression Progression { get; set; }
    [JsonPropertyName("mediaAssets")]
    public MediaAsset[] MediaAssets { get; set; }
    [JsonPropertyName("platforms")]
    public string[] Platforms { get; set; }
    [JsonPropertyName("isSecret")]
    public bool IsSecret { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }
    [JsonPropertyName("lockedDescription")]
    public string LockedDescription { get; set; }
    [JsonPropertyName("productId")]
    public string ProductId { get; set; }
    [JsonPropertyName("achievementType")]
    public string AchievementType { get; set; }
    [JsonPropertyName("participationType")]
    public string ParticipationType { get; set; }
    [JsonPropertyName("timeWindow")]
    public object TimeWindow { get; set; }
    [JsonPropertyName("rewards")]
    public Reward[] Rewards { get; set; }
    [JsonPropertyName("estimatedTime")]
    public string EstimatedTime { get; set; }
    [JsonPropertyName("deeplink")]
    public string Deeplink { get; set; }
    [JsonPropertyName("isRevoked")]
    public bool IsRevoked { get; set; }
    [JsonPropertyName("rarity")]
    public Rarity Rarity { get; set; }
}

public class AchievementSummary
{
    [JsonPropertyName("currentAchievements")]
    public int CurrentAchievements { get; set; }

    [JsonPropertyName("totalAchievements")]
    public int TotalAchievements { get; set; }

    [JsonPropertyName("currentGamerscore")]
    public int CurrentGamerscore { get; set; }

    [JsonPropertyName("totalGamerscore")] public int TotalGamerscore { get; set; }

    [JsonPropertyName("progressPercentage")]
    public int ProgressPercentage { get; set; }

    [JsonPropertyName("sourceVersion")] public int SourceVersion { get; set; }
}