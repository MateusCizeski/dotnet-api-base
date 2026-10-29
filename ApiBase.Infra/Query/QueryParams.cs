using ApiBase.Domain.Exceptions;
using ApiBase.Domain.Query;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiBase.Infra.Query
{
    /// <summary>
    /// Query parameters for paginated, filtered and sorted requests.
    /// All properties follow JSON camelCase convention via JsonPropertyName attributes.
    /// Malformed JSON in filter or sort throws <see cref="InvalidQueryException"/> (HTTP 400).
    /// </summary>
    /// <remarks>
    /// There is no "includes" parameter: the projection to the view already reads the navigation
    /// properties the view declares, and letting the client choose what to load would expose
    /// any navigation of the entity.
    /// </remarks>
    public class QueryParams : QueryField
    {
        /// <summary>Current page number (1-based).</summary>
        [JsonPropertyName("page")]
        public int? Page { get; set; }

        /// <summary>Zero-based start index (alternative to Page; used only when Page is not sent).</summary>
        [JsonPropertyName("start")]
        public int? Start { get; set; }

        /// <summary>Maximum number of records per page. Defaults to 25 and is capped by ApiBaseOptions.MaxPageSize.</summary>
        [JsonPropertyName("limit")]
        public int? Limit { get; set; }

        /// <summary>
        /// JSON array of sort descriptors.
        /// Example: [{"property":"Name","direction":"asc"}]
        /// </summary>
        [JsonPropertyName("sort")]
        public string? Sort { get; set; }

        /// <summary>
        /// JSON array of filter descriptors (flat or grouped).
        /// Flat:    [{"property":"Name","operator":"contains","value":"John"}]
        /// Grouped: [{"filter":"[{...}]","and":true}]
        /// </summary>
        [JsonPropertyName("filter")]
        public string? Filter { get; set; }

        /// <summary>
        /// Creates a QueryParams pre-configured to retrieve a single record by Id.
        /// </summary>
        public static QueryParams FilterById(Guid id)
        {
            var filterList = new[]
            {
                new { property = "Id", @operator = "equal", value = id.ToString() }
            };

            return new QueryParams
            {
                Limit = 1,
                Page = 1,
                Start = 0,
                Filter = JsonSerializer.Serialize(filterList)
            };
        }

        /// <summary>
        /// Deserializes the filter string into a list of FilterGroups.
        /// Supports both flat (array of FilterModel) and grouped (array of FilterGroup) formats.
        /// </summary>
        public List<FilterGroup>? GetFilters()
        {
            if (string.IsNullOrWhiteSpace(Filter))
            {
                return null;
            }

            try
            {
                if (IsGroupedFormat(Filter))
                {
                    var groups = JsonSerializer.Deserialize<List<FilterGroup>>(Filter, JsonOptions) ?? [];

                    foreach (var group in groups)
                    {
                        if (!string.IsNullOrWhiteSpace(group.Filter))
                        {
                            group.Filters = JsonSerializer.Deserialize<List<FilterModel>>(group.Filter, JsonOptions) ?? [];
                        }
                    }

                    return groups;
                }

                var filters = JsonSerializer.Deserialize<List<FilterModel>>(Filter, JsonOptions) ?? [];
                return new List<FilterGroup>
                {
                    new FilterGroup { Filters = filters }
                };
            }
            catch (JsonException ex)
            {
                throw new InvalidQueryException("The 'filter' parameter is not a valid JSON array of filters.", ex);
            }
        }

        /// <summary>
        /// Deserializes the sort string into a list of SortModels.
        /// </summary>
        public List<SortModel>? GetSort()
        {
            if (string.IsNullOrWhiteSpace(Sort))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<List<SortModel>>(Sort, JsonOptions);
            }
            catch (JsonException ex)
            {
                throw new InvalidQueryException("The 'sort' parameter is not a valid JSON array of sort descriptors.", ex);
            }
        }

        /// <summary>
        /// Detects whether the filter JSON is in grouped format (array of FilterGroup)
        /// vs flat format (array of FilterModel).
        /// </summary>
        private static bool IsGroupedFormat(string json)
        {
            try
            {
                var groups = JsonSerializer.Deserialize<List<FilterGroup>>(json, JsonOptions);
                return groups != null && groups.Count > 0 && groups[0].Filter != null;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
