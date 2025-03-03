using System.Text.Json;

namespace Xbl.Client.Tests.Extensions;

public static class ObjectExtensions
{
    public static HttpResponseMessage ToHttpResponseMessage(this object content) =>
        new()
        {
            Content = new StringContent(JsonSerializer.Serialize(content))
        };
}