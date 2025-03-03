using System.Text.Json;

namespace Xbl.Data;

public class JsonItem
{
    public int Id { get; set; }
    public int PartitionKey { get; set; }
    public string Data { get; set; }

    public JsonItem(IHaveId data)
    {
        Id = data.Id;
        if (data is IHavePartitionKey pk) PartitionKey = pk.PartitionKey;
        Data = JsonSerializer.Serialize((object)data);
    }
}