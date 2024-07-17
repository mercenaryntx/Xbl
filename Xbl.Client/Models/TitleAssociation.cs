using System.Text.Json.Serialization;

namespace Xbl.Models;

public class TitleAssociation
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("id")]
    public int Id { get; set; }
}