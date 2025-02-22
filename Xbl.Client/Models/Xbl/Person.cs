using System.Text.Json.Serialization;

namespace Xbl.Client.Models.Xbl;

public class Person
{
    [JsonPropertyName("xuid")]
    public string XUID { get; set; }

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }

    [JsonPropertyName("displayPicRaw")]
    public string DisplayPicRaw { get; set; }

    [JsonPropertyName("gamertag")]
    public string Gamertag { get; set; }

    [JsonPropertyName("gamerScore")]
    public string Gamerscore { get; set; }
}