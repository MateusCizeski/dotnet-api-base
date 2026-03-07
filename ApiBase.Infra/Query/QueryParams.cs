using ApiBase.Domain.Query;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiBase.Infra.Query
{
    /// <summary>
    /// Query parameters for paginated, filtered and sorted requests.
    /// All properties follow JSON camelCase convention via JsonPropertyName attributes.
    /// </summary>
    public class QueryParams : QueryField
    {
        /// <summary>Current page number (1-based).</summary>
        [JsonPropertyName("page")]
        public int? Page { get; set; }

        /// <summary>Zero-based start index (alternative to Page).</summary>
        [JsonPropertyName("start")]
        public int? Start { get; set; }

        /// <summary>Maximum number of records per page. Defaults to 25.</summary>
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
        /// JSON array of navigation properties to eager-load.
        /// Example: ["Address","Orders"]
        /// </summary>
        [JsonPropertyName("includes")]
        public string? Includes { get; set; }

        /// <summary>
        /// Returns the list of navigation properties to eager-load.
        /// </summary>
        public List<string> GetIncludes()
        {
            if (string.IsNullOrWhiteSpace(Includes))
            {
                return [];
            }

            try
            {
                return JsonSerializer.Deserialize<List<string>>(Includes) ?? [];
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error deserializing 'includes': '{Includes}'.", ex);
            }
        }

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
        public List<FilterGroup> GetFilters()
        {
            if (string.IsNullOrWhiteSpace(Filter))
            {
                return null;
            }

            try
            {
                if (IsGroupedFormat(Filter))
                {
                    var groups = JsonSerializer.Deserialize<List<FilterGroup>>(Filter);

                    foreach (var group in groups)
                    {
                        if (!string.IsNullOrWhiteSpace(group.Filter))
                            group.Filters = JsonSerializer.Deserialize<List<FilterModel>>(group.Filter);
                    }

                    return groups;
                }

                var filters = JsonSerializer.Deserialize<List<FilterModel>>(Filter);
                return new List<FilterGroup>
                {
                    new FilterGroup { Filters = filters }
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error deserializing filter string: '{Filter}'.", ex);
            }
        }

        /// <summary>
        /// Deserializes the sort string into a list of SortModels.
        /// </summary>
        public List<SortModel> GetSort()
        {
            if (string.IsNullOrWhiteSpace(Sort))
                return null;

            try
            {
                return JsonSerializer.Deserialize<List<SortModel>>(Sort);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error deserializing sort string: '{Sort}'.", ex);
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
                var groups = JsonSerializer.Deserialize<List<FilterGroup>>(json);
                return groups != null && groups.Count > 0 && groups[0].Filter != null;
            }
            catch
            {
                return false;
            }
        }
    }
}