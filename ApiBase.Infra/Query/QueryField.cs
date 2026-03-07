using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiBase.Infra.Query
{
    /// <summary>
    /// Base class for query parameters that support field selection/projection.
    /// </summary>
    public class QueryField
    {
        /// <summary>
        /// JSON array of field names to include in the response.
        /// Example: ["Id","Name","Email"]
        /// </summary>
        [JsonPropertyName("fields")]
        public string? Fields { get; set; }

        /// <summary>
        /// Returns the list of fields to project, or an empty list if none specified.
        /// </summary>
        public List<string> GetFields()
        {
            if (string.IsNullOrWhiteSpace(Fields))
                return [];

            try
            {
                return JsonSerializer.Deserialize<List<string>>(Fields) ?? [];
            }
            catch
            {
                return [];
            }
        }
    }
}
