using System.Text.Json;

namespace Xbl.Client;

internal static class JsonHelper
{
    public static async Task<T> FromFile<T>(string path)
    {
        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<T>(json);
    }
}