using System.Data;
using System.Text.Json;
using Dapper;
using Xbl.Data.Entities;

namespace Xbl.Data.Repositories;

public class StringKeyedRepository<T>(IDbConnection connection, string tableName) : DapperRepositoryBase<T>(connection, tableName), IRepository<string, T>
    where T : class, IHaveId
{
    public async Task<T> Get(string id, int partitionKey = 0)
    {
        var item = await GetWithTypedContainer<StringKeyedJsonEntity>(new { Id = id, PartitionKey = partitionKey });
        return item == null ? null : JsonSerializer.Deserialize<T>(item.Data);
    }

    public Task<T> Get<TKey>(TKey id, int partitionKey = 0)
    {
        if (id is string stringKey) return Get(stringKey, partitionKey);
        throw new InvalidOperationException("Key can only be string here");
    }

    protected override async Task<IEnumerable<IJsonEntity>> Query(string sql)
    {
        return await Connection.QueryAsync<StringKeyedJsonEntity>(sql);
    }

    protected override IJsonEntity GetContainer(T item) => new StringKeyedJsonEntity((IHaveStringId)item);
}