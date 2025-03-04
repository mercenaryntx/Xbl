using System.Text.Json;

namespace Xbl.Data.Entities;

public class IntKeyedJsonEntity : IHaveIntId, IHavePartitionKey, IJsonEntity
{
    public int Id { get; set; }
    public int PartitionKey { get; set; }
    public DateTime UpdatedOn { get; set; }
    public string Data { get; set; }

    public IntKeyedJsonEntity() {}

    public IntKeyedJsonEntity(IHaveIntId data)
    {
        Id = data.Id;
        UpdatedOn = DateTime.UtcNow;
        if (data is IHavePartitionKey pk) PartitionKey = pk.PartitionKey;
        Data = JsonSerializer.Serialize((object)data);
    }
}