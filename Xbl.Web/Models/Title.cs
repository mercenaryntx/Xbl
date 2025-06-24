using System.Text.Json.Serialization;
using Xbl.Client.Models.Kql;

namespace Xbl.Web.Models;

public class Title
{
    [JsonPropertyName("titleId")]
    public string TitleId { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("displayImage")]
    public string DisplayImage { get; set; }
    [JsonPropertyName("currentAchievements")]
    public int CurrentAchievements { get; set; }
    [JsonPropertyName("totalAchievements")]
    public int TotalAchievements { get; set; }
    [JsonPropertyName("currentGamerscore")]
    public int CurrentGamerscore { get; set; }
    [JsonPropertyName("totalGamerscore")]
    public int TotalGamerscore { get; set; }
    [JsonPropertyName("progressPercentage")]
    public int ProgressPercentage { get; set; }
    [JsonPropertyName("lastTimePlayed")]
    public DateTime LastTimePlayed { get; set; }
}

public class TitleDetail : Title
{
    [JsonPropertyName("achievements")]
    public Achievement[] Achievements { get; set; }
    [JsonPropertyName("minutes")]
    public int? Minutes { get; set; }
}