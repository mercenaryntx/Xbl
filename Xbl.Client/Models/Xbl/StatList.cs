using System.Text.Json.Serialization;

namespace Xbl.Client.Models.Xbl;

public class StatList
{
    [JsonPropertyName("stats")]
    public Stat[] Stats { get; set; }

    [JsonPropertyName("arrangebyfield")]
    public string ArrangeByField { get; set; }

    [JsonPropertyName("arrangebyfieldid")]
    public string ArrangeByFieldId { get; set; }
}