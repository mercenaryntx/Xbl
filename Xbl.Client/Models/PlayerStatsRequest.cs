using System.Text.Json.Serialization;

namespace Xbl.Client.Models;

public class PlayerStatsRequest
{
    [JsonPropertyName("xuids")]
    public string[] XUIDs { get; set; }

    [JsonPropertyName("groups")]
    public TitleStatGroup[] Groups { get; set; }

    [JsonPropertyName("stats")]
    public Stat[] Stats { get; set; }
}