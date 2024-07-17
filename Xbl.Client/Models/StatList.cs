using System.Text.Json.Serialization;

namespace Xbl.Models;

public class StatList
{
    [JsonPropertyName("stats")]
    public Stat[] Stats { get; set; }

    [JsonPropertyName("arrangebyfield")]
    public string ArrangeByField { get; set; }

    [JsonPropertyName("arrangebyfieldid")]
    public string ArrangeByFieldId { get; set; }
}