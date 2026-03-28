using System.Text.Json.Serialization;

namespace Xbl.Client.Models.Xbl.Achievements;

public class ContentResponse<T>
{
    [JsonPropertyName("content")]
    public T Content { get; set; }
}