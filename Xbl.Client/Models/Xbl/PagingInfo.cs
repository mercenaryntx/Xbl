using System.Text.Json.Serialization;

namespace Xbl.Client.Models.Xbl
{
    public class PagingInfo
    {
        [JsonPropertyName("continuationToken")]
        public object ContinuationToken { get; set; }
        [JsonPropertyName("totalRecords")]
        public int TotalRecords { get; set; }
    }
}
