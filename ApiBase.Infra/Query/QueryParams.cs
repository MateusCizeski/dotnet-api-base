using ApiBase.Domain.Query;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace ApiBase.Infra.Query
{
    public class QueryParams : QueryField
    {
        [JsonPropertyName("page")]
        public int? Page { get; set; }

        [JsonPropertyName("start")]
        public int? Start { get; set; }

        [JsonPropertyName("limit")]
        public int? Limit { get; set; }

        [JsonPropertyName("sort")]
        public string Sort { get; set; }

        [JsonPropertyName("filter")]
        public string Filter { get; set; }

        [JsonPropertyName("includes")]
        public string Includes { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public int? page { get => Page; set => Page = value; }
        [System.Text.Json.Serialization.JsonIgnore]
        public int? limit { get => Limit; set => Limit = value; }

        public List<string> GetIncludes()
        {
            if (string.IsNullOrWhiteSpace(Includes))
            {
                return new List<string>();
            }

            try
            {
                return JsonConvert.DeserializeObject<List<string>>(Includes) ?? new List<string>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deserializing 'includes': '{Includes}'. Details: {ex.Message}", ex);
            }
        }

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
                Filter = JsonConvert.SerializeObject(filterList)
            };
        }

        public List<FilterGroup> GetFilters()
        {
            if (string.IsNullOrWhiteSpace(Filter))
            {
                return null;
            }

            try
            {
                var token = Filter.TrimStart();

                if (token.StartsWith("[{") && IsGroupedFormat(Filter))
                {
                    var groups = JsonConvert.DeserializeObject<List<FilterGroup>>(Filter);

                    foreach (var group in groups)
                    {
                        if (!string.IsNullOrWhiteSpace(group.Filter))
                        {
                            group.Filters = JsonConvert.DeserializeObject<List<FilterModel>>(group.Filter);
                        }
                    }

                    return groups;
                }
                else
                {
                    var filters = JsonConvert.DeserializeObject<List<FilterModel>>(Filter);
                    return new List<FilterGroup>
                    {
                        new FilterGroup { Filters = filters }
                    };
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deserializing filter string: '{Filter}'. Details: {ex.Message}", ex);
            }
        }

        public List<SortModel> GetSort()
        {
            if (string.IsNullOrWhiteSpace(Sort))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<List<SortModel>>(Sort);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deserializing sort string: '{Sort}'. Details: {ex.Message}", ex);
            }
        }

        private static bool IsGroupedFormat(string json)
        {
            try
            {
                var groups = JsonConvert.DeserializeObject<List<FilterGroup>>(json);
                return groups != null && groups.Count > 0 && groups[0].Filter != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
