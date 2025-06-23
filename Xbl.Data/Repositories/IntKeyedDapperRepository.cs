using System.Data;
using System.Text.Json;
using Dapper;
using Xbl.Data.Entities;

namespace Xbl.Data.Repositories;

public class IntKeyedDapperRepository<T>(IDbConnection connection, string tableName) : DapperRepositoryBase<T>(connection, tableName), IRepository<int, T>
    where T : class, IHaveId
{
    public async Task<T> Get(int id, int partitionKey = 0)
    {
        var item = await GetWithTypedContainer<IntKeyedJsonEntity>(new { Id = id, PartitionKey = partitionKey });
        return item == null ? null : JsonSerializer.Deserialize<T>(item.Data);
    }

    public Task<T> Get<TKey>(TKey id, int partitionKey = 0)
    {
        if (id is int intKey) return Get(intKey, partitionKey);
        throw new InvalidOperationException("Key can only be int here");
    }

    public Task<IEnumerable<T>> GetPartition(int partitionKey)
    {
        return GetPartitionInner(partitionKey);
    }

    protected override async Task<IEnumerable<IJsonEntity>> Query(string sql)
    {
        return await Connection.QueryAsync<IntKeyedJsonEntity>(sql);
    }

    protected override IJsonEntity GetContainer(T item) => new IntKeyedJsonEntity((IHaveIntId)item);
}