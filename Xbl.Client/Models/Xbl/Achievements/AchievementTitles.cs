using System.Text.Json.Serialization;

namespace Xbl.Client.Models.Xbl.Achievements;

public class AchievementTitles
{
    [JsonPropertyName("xuid")]
    public string Xuid { get; set; }
    [JsonPropertyName("titles")]
    public Title[] Titles { get; set; }
}