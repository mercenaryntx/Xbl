using System.Text.Json;

namespace Xbl.Data.Entities;

public class StringKeyedJsonEntity : IHaveStringId, IHavePartitionKey, IJsonEntity
{
    public string Id { get; set; }
    public int PartitionKey { get; set; }
    public DateTime UpdatedOn { get; set; }
    public string Data { get; set; }

    public StringKeyedJsonEntity()
    {
    }

    public StringKeyedJsonEntity(IHaveStringId data)
    {
        Id = data.Id;
        UpdatedOn = DateTime.UtcNow;
        if (data is IHavePartitionKey pk) PartitionKey = pk.PartitionKey;
        Data = JsonSerializer.Serialize((object)data);
    }
}