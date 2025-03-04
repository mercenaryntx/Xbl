using System.Data;
using System.Text.Json;
using Dapper;
using Xbl.Data.Entities;

namespace Xbl.Data.Repositories;

public abstract class DapperRepositoryBase<T> where T : class, IHaveId
{
    protected readonly IDbConnection Connection;
    private readonly string _tableName;

    protected DapperRepositoryBase(IDbConnection connection, string tableName)
    {
        Connection = connection;
        _tableName = tableName;
    }

    public Task Truncate()
    {
        var sql = $"TRUNCATE TABLE {_tableName}";
        return Connection.ExecuteAsync(sql);
    }

    public Task Insert(T item)
    {
        var sql = $"INSERT INTO {_tableName} (Id, PartitionKey, UpdatedOn, Data) VALUES (@Id, @PartitionKey, @UpdatedOn, @Data)";
        return Connection.ExecuteAsync(sql, GetContainer(item));
    }

    public Task BulkInsert(IEnumerable<T> items)
    {
        var sql = $"INSERT INTO {_tableName} (Id, PartitionKey, UpdatedOn, Data) VALUES (@Id, @PartitionKey, @UpdatedOn, @Data)";
        return Connection.ExecuteAsync(sql, items.Select(GetContainer));
    }

    public Task Update(T item)
    {
        var sql = $"UPDATE {_tableName} SET Data = @Data, UpdatedOn = @UpdatedOn WHERE Id = @Id AND PartitionKey = @PartitionKey";
        return Connection.ExecuteAsync(sql, GetContainer(item));
    }

    public async Task BulkUpdate(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            await Update(item);
        }
    }

    public Task Delete(T item)
    {
        var sql = $"DELETE FROM {_tableName} WHERE Id = @Id AND PartitionKey = @PartitionKey";
        return Connection.ExecuteAsync(sql, GetContainer(item));
    }

    public async Task<IEnumerable<T>> GetAll()
    {
        var items = await Query($"SELECT Data FROM {_tableName}");
        return items.Select(i => JsonSerializer.Deserialize<T>(i.Data));
    }

    public async Task<IEnumerable<IJsonEntity>> GetHeaders()
    {
        return await Query($"SELECT Id, PartitionKey, UpdatedOn FROM {_tableName}");
    }

    protected async Task<TContainer> GetWithTypedContainer<TContainer>(object key) where TContainer : IJsonEntity
    {
        var sql = $"SELECT Data FROM {_tableName} WHERE Id = @Id AND PartitionKey = @PartitionKey";
        return await Connection.QueryFirstOrDefaultAsync<TContainer>(sql, key);
    }

    protected abstract IJsonEntity GetContainer(T item);
    protected abstract Task<IEnumerable<IJsonEntity>> Query(string sql);

}