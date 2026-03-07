namespace ApiBase.Domain.Enums
{
    /// <summary>
    /// Defines the comparison operator used in a filter condition.
    /// Used by <see cref="ApiBase.Domain.Query.FilterModel"/> to determine
    /// how the property value is compared against the filter value.
    /// </summary>
    public enum FilterOperator
    {
        /// <summary>Exact match. Equivalent to SQL <c>=</c>.</summary>
        Equal,

        /// <summary>Non-match. Equivalent to SQL <c>!=</c>.</summary>
        NotEqual,

        /// <summary>Strictly greater than. Equivalent to SQL <c>&gt;</c>.</summary>
        GreaterThan,

        /// <summary>Greater than or equal to. Equivalent to SQL <c>&gt;=</c>.</summary>
        GreaterThanOrEqual,

        /// <summary>Strictly less than. Equivalent to SQL <c>&lt;</c>.</summary>
        LessThan,

        /// <summary>Less than or equal to. Equivalent to SQL <c>&lt;=</c>.</summary>
        LessThanOrEqual,

        /// <summary>String contains the value (case-insensitive).</summary>
        Contains,

        /// <summary>String starts with the value.</summary>
        StartsWith,

        /// <summary>String ends with the value.</summary>
        EndsWith,

        /// <summary>Value is in the provided list. Equivalent to SQL <c>IN (...)</c>.</summary>
        In,

        /// <summary>Value is in the provided list, or the property is null.</summary>
        InOrNull,

        /// <summary>All values in the provided list are contained in the property string.</summary>
        ContainsAll
    }
}
