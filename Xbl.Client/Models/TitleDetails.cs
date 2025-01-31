using System.Text.Json.Serialization;

namespace Xbl.Client.Models;

public class TitleDetails
{
    [JsonPropertyName("achievements")]
    public Achievement[] Achievements { get; set; }
    [JsonPropertyName("pagingInfo")]
    public PagingInfo PagingInfo { get; set; }
}