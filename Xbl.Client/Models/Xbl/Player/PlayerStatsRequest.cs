using System.Text.Json.Serialization;

namespace Xbl.Client.Models.Xbl.Player;

public class PlayerStatsRequest
{
    [JsonPropertyName("xuids")]
    public string[] XUIDs { get; set; }

    [JsonPropertyName("stats")]
    public Stat[] Stats { get; set; }
}