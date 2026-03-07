using System.Text.Json.Serialization;

namespace ApiBase.Domain.Query
{
    public class FilterGroup
    {
        [JsonPropertyName("filter")]
        public string Filter { get; set; }

        public List<FilterModel> Filters { get; set; }

        public bool And { get; set; } = true;
    }
}
