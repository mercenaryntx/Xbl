using System.Data;
using System.Text.Json;
using Dapper;

namespace Xbl.Data;

public class DataRepository<T> : IDataRepository<T> where T : class, IHaveId
{
    private readonly IDbConnection _connection;
    private readonly string _tableName;

    public DataRepository(IDbConnection connection, string tableName)
    {
        _connection = connection;
        _tableName = tableName;
    }

    public Task Insert(T item, string table = null)
    {
        table ??= _tableName;
        var sql = $"INSERT INTO {table} (Id, PartitionKey, Data) VALUES (@Id, @PartitionKey, @Data)";
        return _connection.ExecuteAsync(sql, new JsonItem(item));
    }

    public Task BulkInsert(IEnumerable<T> items, string table = null)
    {
        table ??= _tableName;
        var sql = $"INSERT INTO {table} (Id, PartitionKey, Data) VALUES (@Id, @PartitionKey, @Data)";
        return _connection.ExecuteAsync(sql, items.Select(item => new JsonItem(item)));
    }

    public Task Update(T item, string table = null)
    {
        table ??= _tableName;
        var sql = $"UPDATE {table} SET Data = @Data WHERE Id = @Id AND PartitionKey = @PartitionKey";
        return _connection.ExecuteAsync(sql, new JsonItem(item));
    }

    public Task Delete(T item, string table = null)
    {
        table ??= _tableName;
        var sql = $"DELETE FROM {table} WHERE Id = @Id AND PartitionKey = @PartitionKey";
        return _connection.ExecuteAsync(sql, new JsonItem(item));
    }

    public async Task<T> Get(int id, int partitionKey = 0, string table = null)
    {
        table ??= _tableName;
        var sql = $"SELECT Id, Data FROM {table} WHERE Id = @Id AND PartitionKey = @PartitionKey";
        var item = await _connection.QueryFirstOrDefaultAsync<JsonItem>(sql, new { Id = id, PartitionKey = partitionKey });
        return JsonSerializer.Deserialize<T>(item.Data);
    }

    public async Task<IEnumerable<T>> GetAll(string table = null)
    {
        table ??= _tableName;
        var sql = $"SELECT Id, PartitionKey, Data FROM {table}";
        var items = await _connection.QueryAsync<JsonItem>(sql);
        return items.Select(i => JsonSerializer.Deserialize<T>(i.Data));
    }
}