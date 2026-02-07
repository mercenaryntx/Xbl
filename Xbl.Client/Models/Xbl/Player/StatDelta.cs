using System.Text.Json.Serialization;
using Xbl.Data.Entities;

namespace Xbl.Client.Models.Xbl.Player;

public class StatDelta : IHaveIntId, IHavePartitionKey
{
    [JsonPropertyName("titleid")]
    public int TitleId { get; set; }

    [JsonPropertyName("minutes")]
    public int Minutes { get; set; }

    [JsonIgnore]
    public int Id { get; set; }

    [JsonIgnore]
    public int PartitionKey => TitleId;
}

