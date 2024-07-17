using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Xbl.Models
{
    public class PagingInfo
    {
        [JsonPropertyName("continuationToken")]
        public object ContinuationToken { get; set; }
        [JsonPropertyName("totalRecords")]
        public int TotalRecords { get; set; }
    }
}
