using System.Text.Json.Serialization;

namespace Xbl.Client.Models.Xbl.Achievements
{
    public class Image
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }

}
