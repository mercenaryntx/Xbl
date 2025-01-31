using System.Text.Json.Serialization;

namespace Xbl.Client.Models;

public class Requirement
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("current")]
    public string Current { get; set; }
    [JsonPropertyName("target")]
    public string Target { get; set; }
    [JsonPropertyName("operationType")]
    public string OperationType { get; set; }
    [JsonPropertyName("valueType")]
    public string ValueType { get; set; }
    [JsonPropertyName("ruleParticipationType")]
    public string RuleParticipationType { get; set; }
}