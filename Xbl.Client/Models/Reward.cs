using System.Text.Json.Serialization;

namespace Xbl.Client.Models;

public class Reward
{
    [JsonPropertyName("name")]
    public object Name { get; set; }
    [JsonPropertyName("description")]
    public object Description { get; set; }
    [JsonPropertyName("value")]
    public string Value { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }
    [JsonPropertyName("mediaAsset")]
    public object MediaAsset { get; set; }
    [JsonPropertyName("valueType")]
    public string ValueType { get; set; }

    public int IntValue => ValueType == "Int" && int.TryParse(Value, out var x) ? x : 0;
}