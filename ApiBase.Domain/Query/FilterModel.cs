using ApiBase.Domain.Enums;
using System.Text.Json.Serialization;

namespace ApiBase.Domain.Query
{
    /// <summary>
    /// Defines a single filter condition to be applied to a query.
    /// </summary>
    public class FilterModel
    {
        public const string OpEquals = "equal";
        public const string OpGreater = "greater";
        public const string OpLess = "less";
        public const string OpContains = "contains";
        public const string OpContainsAll = "containsall";
        public const string OpIn = "in";
        public const string OpGreaterOrEqual = "greaterOrEqual";
        public const string OpLessOrEqual = "lessOrEqual";
        public const string OpStartsWith = "startswith";
        public const string OpEndsWith = "endswith";
        public const string OpInOrNull = "inOrNull";
        public const string OpNotEqual = "notEqual";

        /// <summary>
        /// Dot-notation property path to filter on. Example: "Address.City"
        /// </summary>
        [JsonPropertyName("property")]
        public string Property { get; set; }

        /// <summary>
        /// The value to compare against.
        /// </summary>
        [JsonPropertyName("value")]
        public object Value { get; set; }

        /// <summary>
        /// String representation of the operator, used when deserializing from JSON.
        /// Takes lower priority than <see cref="Operator"/> when both are set.
        /// </summary>
        [JsonPropertyName("operator")]
        public string OperatorString { get; set; } = OpEquals;

        /// <summary>
        /// Strongly-typed operator. Takes priority over <see cref="OperatorString"/> when set.
        /// </summary>
        public FilterOperator? Operator { get; set; }

        /// <summary>Whether this filter combines with the next using AND (true) or OR (false).</summary>
        public bool And { get; set; } = true;

        /// <summary>Whether to negate this filter condition.</summary>
        public bool Not { get; set; } = false;

        /// <summary>Marks this filter as the primary/main filter for the query.</summary>
        public bool MainFilter { get; set; }

        /// <summary>
        /// Returns the final segment of the property path.
        /// For "Address.City" returns "City".
        /// </summary>
        public string PropertyName
        {
            get
            {
                var parts = (Property ?? string.Empty).Split('.');
                return parts.Last();
            }
        }

        public FilterModel() { }

        /// <summary>
        /// Resolves the effective filter operator, giving priority to the enum value over the string.
        /// </summary>
        public FilterOperator GetOperator()
        {
            if (Operator.HasValue)
            {
                return Operator.Value;
            }

            return OperatorString switch
            {
                OpEquals => FilterOperator.Equal,
                OpNotEqual => FilterOperator.NotEqual,
                OpGreater => FilterOperator.GreaterThan,
                OpLess => FilterOperator.LessThan,
                OpContains => FilterOperator.Contains,
                OpContainsAll => FilterOperator.ContainsAll,
                OpIn => FilterOperator.In,
                OpGreaterOrEqual => FilterOperator.GreaterThanOrEqual,
                OpLessOrEqual => FilterOperator.LessThanOrEqual,
                OpStartsWith => FilterOperator.StartsWith,
                OpEndsWith => FilterOperator.EndsWith,
                OpInOrNull => FilterOperator.InOrNull,
                _ => throw new NotSupportedException($"Unknown filter operator: '{OperatorString}'"),
            };
        }
    }
}
