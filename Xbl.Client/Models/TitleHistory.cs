using System.Text.Json.Serialization;

namespace Xbl.Models;

public class TitleHistory
{
    [JsonPropertyName("lastTimePlayed")]
    public DateTime LastTimePlayed { get; set; }
    [JsonPropertyName("visible")]
    public bool Visible { get; set; }
    [JsonPropertyName("canHide")]
    public bool CanHide { get; set; }
}

