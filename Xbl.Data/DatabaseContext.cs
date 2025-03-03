using System.Data;
using System.Reflection;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Xbl.Data;

public class DatabaseContext(string db) : IDatabaseContext
{
    public IDbConnection Connection { get; } = new SqliteConnection($"Data Source={Path.Combine("data", db)}.db");
    public Dictionary<Type, Container> Containers { get; } = AllContainers.Where(c => c.Value.Databases.Contains(db)).ToDictionary();
    
    public Task CreateTables()
    {
        var tasks = Containers.Select(c => Connection.ExecuteAsync(CreateTableScript(c.Value.Name)));
        return Task.WhenAll(tasks);
    }

    public IDataRepository<T> GetRepository<T>() where T : class, IHaveId
    {
        return new DataRepository<T>(Connection, Containers[typeof(T)].Name);
    }

    private static string CreateTableScript(string name)
    {
        return $"CREATE TABLE IF NOT EXISTS {name} (Id INTEGER NOT NULL, PartitionKey INTEGER NOT NULL, Data JSON, PRIMARY KEY(Id, PartitionKey))"
    }

    private static Dictionary<Type, Container> AllContainers { get; } = new();

    static DatabaseContext()
    {
        var types = Assembly.GetEntryAssembly().GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Contains(typeof(IHaveId)));

        foreach (var type in types)
        {
            var databaseAttribute = type.GetCustomAttribute<DatabaseAttribute>();
            if (databaseAttribute == null) throw new InvalidOperationException($"Type {type.Name} does not have a DatabaseAttribute.");
            AllContainers.Add(type, new Container { Name = type.Name.ToLower(), Databases = [..databaseAttribute.Databases]});
        }
    }
}