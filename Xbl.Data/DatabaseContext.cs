using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;
using Xbl.Data.Entities;
using Xbl.Data.Repositories;

namespace Xbl.Data;

public class DatabaseContext : IDatabaseContext, IDisposable
{
    private const string MEMORY = ":memory";
    private readonly string _source;
    private SqliteConnectionStringBuilder _connectionStringBuilder;

    private IDbConnection _connection;
    public IDbConnection Connection => _connection ??= CreateConnection(_source, SqliteOpenMode.ReadOnly);

    public bool IsExists => _source == MEMORY || File.Exists(_source);
    public bool IsVoid => _connectionStringBuilder.DataSource != _source;
    public bool IsReadOnly => _connectionStringBuilder.Mode == SqliteOpenMode.ReadOnly;

    //For testing purposes only
    public DatabaseContext()
    {
        _source = MEMORY;
    }

    public DatabaseContext(string db)
    {
        _source = Path.Combine("data", db) + ".db";
    }

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

    public async Task<IEnumerable<T>> Query<T>(string query, object param = null)
    {
        return IsVoid ? Array.Empty<T>() : await Connection.QueryAsync<T>(query, param);
    }

    public Task EnsureTable(string tableName)
    {
        return IsReadOnly ? Task.CompletedTask : Connection.ExecuteAsync(CreateTableScript(tableName));
    }

    public IDatabaseContext Mandatory(SqliteOpenMode mode = SqliteOpenMode.ReadOnly)
    {
        _connection ??= CreateConnection(_source, mode);
        return this;
    }

    public IDatabaseContext Optional()
    {
        _connection ??= IsExists
            ? CreateConnection(_source, SqliteOpenMode.ReadOnly)
            : CreateConnection(MEMORY, SqliteOpenMode.Memory);
        return this;
    }

    private SqliteConnection CreateConnection(string source, SqliteOpenMode mode)
    {
        if (source == MEMORY) mode = SqliteOpenMode.Memory;
        var b = new SqliteConnectionStringBuilder
        {
            DataSource = source,
            Mode = mode
        };
        var connection = new SqliteConnection(b.ConnectionString);
        connection.Open();
        _connectionStringBuilder = b;
        return connection;
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
        _connection?.Dispose();
    }
}