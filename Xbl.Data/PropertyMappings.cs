using MicroOrm.Dapper.Repositories.SqlGenerator;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Xbl.Data;

public static class PropertyMappings
{
    private static readonly Dictionary<Type, Dictionary<string, SqlPropertyMetadata>> Mappings = new();

    public static Dictionary<string, SqlPropertyMetadata> GetMapping<TEntity>()
    {
        var type = typeof(TEntity);
        if (!Mappings.ContainsKey(type))
        {
            Mappings[type] = CreatePropertyNameMappingTable(type);
        }
        return Mappings[type];
    }

    private static Dictionary<string, SqlPropertyMetadata> CreatePropertyNameMappingTable(Type type)
    {
        return type.GetProperties().ToDictionary(pi => pi.Name, pi =>
        {
            var jsonPropertyNameAttribute = pi.GetCustomAttribute<JsonPropertyNameAttribute>();
            var columnName = jsonPropertyNameAttribute?.Name ?? pi.Name;
            return new SqlPropertyMetadata(pi)
            {
                Alias = pi.Name,
                ColumnName = $"json_extract(Data, '$.{columnName}')",
                CleanColumnName = columnName
            };
        });
    }

}