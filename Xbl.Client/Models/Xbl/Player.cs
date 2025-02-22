using System.Text.Json.Serialization;

namespace Xbl.Client.Models.Xbl;

public class Player
{
    [JsonPropertyName("people")]
    public Person[] People { get; set; }
}