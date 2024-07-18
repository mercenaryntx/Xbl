using System.Text.Json.Serialization;

namespace Xbl.Client.Models;

public class Player
{
    [JsonPropertyName("people")]
    public Person[] People { get; set; }
}