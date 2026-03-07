using System.Text.Json.Serialization;

namespace ApiBase.Domain.Query
{
    /// <summary>
    /// Defines a sorting rule for a query, specifying the property and direction.
    /// </summary>
    public class SortModel
    {
        /// <summary>
        /// Optional filter value used for conditional ordering (e.g. pin a specific value to the top).
        /// </summary>
        [JsonPropertyName("filterValue")]
        public object? FilterValue { get; set; }

        /// <summary>
        /// Dot-notation property path to sort by. Example: "Address.City"
        /// </summary>
        [JsonPropertyName("property")]
        public string Property { get; set; }

        /// <summary>
        /// Sort direction: "asc" or "desc".
        /// </summary>
        [JsonPropertyName("direction")]
        public string Direction { get; set; }

        /// <summary>
        /// Returns true if the sort direction is ascending.
        /// </summary>
        public bool IsAscending() => Direction?.ToLower() == "asc";

        /// <summary>
        /// Kept for internal compatibility. Prefer IsAscending().
        /// </summary>
        internal bool ASC() => IsAscending();
    }
}
