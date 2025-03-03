using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Xbl.Client.Repositories;

[ExcludeFromCodeCoverage]
public abstract class RepositoryBase : IRepository
{
    public async Task<T> LoadJson<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<T>(json);
    }

    public async Task SaveJson(string path, string json)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
        await File.WriteAllTextAsync(path, json);

    }

    public Task SaveJson(string path, object data)
    {
        var json = JsonSerializer.Serialize(data);
        return SaveJson(path, json);
    }
}