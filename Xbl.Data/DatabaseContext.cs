using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Xbl.Data.Entities;
using Xbl.Data.Repositories;

namespace Xbl.Data;

public class DatabaseContext : IDatabaseContext, IDisposable
{
    //For testing purposes only
    public DatabaseContext()
    {
        Connection = new SqliteConnection("Data Source=:memory:");
        Connection.Open();
    }

    public DatabaseContext(string db)
    {
        Connection = new SqliteConnection($"Data Source={Path.Combine("data", db)}.db");
        //Connection.Open();
    }

    public IDbConnection Connection { get; }
    
    public async Task<IRepository<T>> GetRepository<T>(string tableName = null) where T : class, IHaveId
    {
        var type = typeof(T);
        tableName ??= type.Name.ToLower();
        await EnsureTable(tableName);

        var interfaces = type.GetInterfaces();
        if (interfaces.Contains(typeof(IHaveIntId))) return new IntKeyedDapperRepository<T>(Connection, tableName);
        if (interfaces.Contains(typeof(IHaveStringId))) return new StringKeyedRepository<T>(Connection, tableName);
        throw new InvalidOperationException("Type must implement either `IHaveIntId` or `IHaveStringId`");
    }

    public Task<IEnumerable<T>> Query<T>(string query, object param = null)
    {
        return Connection.QueryAsync<T>(query, param);
    }

    public Task EnsureTable(string tableName)
    {
        return Connection.ExecuteAsync(CreateTableScript(tableName));
    }

    private static string CreateTableScript(string name) 
        => $"""
            CREATE TABLE IF NOT EXISTS {name} (
                Id INTEGER NOT NULL, 
                PartitionKey INTEGER NOT NULL, 
                UpdatedOn DATETIME NOT NULL,
                Data JSON, 
                PRIMARY KEY(Id, PartitionKey)
            )
            """;

    public void Dispose()
    {
        Connection?.Dispose();
    }
}