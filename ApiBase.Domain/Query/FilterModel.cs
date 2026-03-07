using ApiBase.Domain.Enums;
using System.Text.Json.Serialization;

namespace ApiBase.Domain.Query
{
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

        [JsonPropertyName("property")]
        public string Property { get; set; }

        [JsonPropertyName("value")]
        public object Value { get; set; }

        [JsonPropertyName("operator")]
        public string OperatorString { get; set; } = OpEquals;
        public FilterOperator? Operator { get; set; }
        public bool And { get; set; } = true;
        public bool Not { get; set; } = false;
        public bool MainFilter { get; set; }

        public string PropertyName
        {
            get
            {
                var parts = (Property ?? string.Empty).Split('.');
                return parts.Last();
            }
        }

        public FilterModel() { }

        public FilterOperator GetOperator()
        {
            if (Operator.HasValue)
            {
                return Operator.Value;
            }

            return OperatorString switch
            {
                OpEquals => FilterOperator.Equal,
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
