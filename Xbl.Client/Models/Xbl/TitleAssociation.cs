using System.Text.Json.Serialization;

namespace Xbl.Client.Models.Xbl;

public class TitleAssociation
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("id")]
    public long Id { get; set; }
}