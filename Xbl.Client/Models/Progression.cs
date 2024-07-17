using System.Text.Json.Serialization;

namespace Xbl.Models;

public class Progression
{
    [JsonPropertyName("requirements")]
    public Requirement[] Requirements { get; set; }
    [JsonPropertyName("timeUnlocked")]
    public DateTime TimeUnlocked { get; set; }
}