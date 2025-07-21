using System.Text.Json.Serialization;

namespace Xbl.Web.Models;

public class TitleDetail : Title
{
    [JsonPropertyName("achievements")]
    public Achievement[] Achievements { get; set; }
    [JsonPropertyName("minutes")]
    public int? Minutes { get; set; }
}