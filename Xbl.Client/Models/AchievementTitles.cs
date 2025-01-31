using System.Text.Json.Serialization;

namespace Xbl.Client.Models;

public class AchievementTitles
{
    [JsonPropertyName("xuid")]
    public string Xuid { get; set; }
    [JsonPropertyName("titles")]
    public Title[] Titles { get; set; }
}