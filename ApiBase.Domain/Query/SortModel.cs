using System.Text.Json.Serialization;

namespace ApiBase.Domain.Query
{
    public class SortModel
    {
        [JsonPropertyName("filterValue")]
        public object FilterValue { get; set; }

        [JsonPropertyName("property")]
        public string Property { get; set; }

        [JsonPropertyName("direction")]
        public string Direction { get; set; }

        public bool ASC()
        {
            return Direction?.ToLower() == "asc";
        }
    }
}
