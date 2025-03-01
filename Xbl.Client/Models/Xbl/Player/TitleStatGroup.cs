using System.Text.Json.Serialization;

namespace Xbl.Client.Models.Xbl.Player;

public class TitleStatGroup
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("titleid")]
    public string TitleId { get; set; }

    [JsonPropertyName("statlistscollection")]
    public StatList[] StatListsCollection { get; set; }
}