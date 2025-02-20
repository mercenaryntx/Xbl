using System.Text.Json.Serialization;

namespace Xbl.Client.Models;

public class TitleDetails<T>
{
    [JsonPropertyName("achievements")]
    public T[] Achievements { get; set; }
    [JsonPropertyName("pagingInfo")]
    public PagingInfo PagingInfo { get; set; }
}