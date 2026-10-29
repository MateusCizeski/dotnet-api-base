using ApiBase.Domain.Exceptions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiBase.Infra.Query
{
    /// <summary>
    /// Base class for query parameters that support field selection/projection.
    /// </summary>
    public class QueryField
    {
        /// <summary>Case-insensitive JSON options shared by the query parameter parsers.</summary>
        protected static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        /// <summary>
        /// Fields to include in the response, as a JSON array (["Id","Name"]) or a comma list (Id,Name).
        /// </summary>
        [JsonPropertyName("fields")]
        public string? Fields { get; set; }

        /// <summary>
        /// Returns the list of fields to project, or an empty list if none specified.
        /// </summary>
        public List<string> GetFields()
        {
            if (string.IsNullOrWhiteSpace(Fields))
            {
                return [];
            }

            if (!Fields.TrimStart().StartsWith('['))
            {
                return Fields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            }

            try
            {
                return JsonSerializer.Deserialize<List<string>>(Fields, JsonOptions) ?? [];
            }
            catch (JsonException ex)
            {
                throw new InvalidQueryException("The 'fields' parameter is not a valid JSON array of field names.", ex);
            }
        }
    }
}
