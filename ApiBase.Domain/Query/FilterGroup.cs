using System.Text.Json.Serialization;

namespace ApiBase.Domain.Query
{
    /// <summary>
    /// Groups multiple filter conditions that are evaluated together.
    /// Groups themselves can be combined with AND or OR logic.
    /// </summary>
    public class FilterGroup
    {
        /// <summary>
        /// Raw JSON string of the inner filters (used in grouped format deserialization).
        /// </summary>
        [JsonPropertyName("filter")]
        public string? Filter { get; set; }

        /// <summary>
        /// The deserialized list of filter conditions in this group.
        /// </summary>
        public List<FilterModel> Filters { get; set; }

        /// <summary>
        /// Whether this group combines with the next using AND (true) or OR (false). Defaults to AND.
        /// </summary>
        public bool And { get; set; } = true;
    }
}
