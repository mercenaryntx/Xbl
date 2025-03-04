namespace Xbl.Data.Entities;

public interface IHavePartitionKey
{
    int PartitionKey { get; }
}