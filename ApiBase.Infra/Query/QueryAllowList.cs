using ApiBase.Domain.Exceptions;
using ApiBase.Domain.Query;
using System.Collections;
using System.Reflection;

namespace ApiBase.Infra.Query
{
    /// <summary>
    /// Validates that filters and sorts only reference properties the client is allowed to see.
    /// Filters run against the entity (so EF Core can translate them to SQL), but the allowed
    /// paths come from the view: a property that is not in the view cannot be filtered or sorted,
    /// otherwise a client could probe hidden columns (e.g. "startswith" on a password hash) — OWASP API3.
    /// Dot paths are checked segment by segment through the nested views
    /// ("Invoice.Seller.FullName" needs Invoice → Seller → FullName to exist in the view types).
    /// </summary>
    public static class QueryAllowList
    {
        /// <summary>
        /// Throws <see cref="InvalidQueryException"/> when a filter or sort uses a property path
        /// that does not exist in <paramref name="viewType"/>.
        /// </summary>
        public static void Validate(IEnumerable<FilterGroup>? filters, IEnumerable<SortModel>? sorts, Type viewType)
        {
            foreach (var filter in (filters ?? []).SelectMany(g => g.Filters ?? []))
            {
                EnsureAllowed(filter.Property, viewType, "filter");
            }

            foreach (var sort in sorts ?? [])
            {
                EnsureAllowed(sort.Property, viewType, "sort");
            }
        }

        /// <summary>
        /// Returns true when every segment of <paramref name="propertyPath"/> is a public property
        /// of the view (or of the nested view / collection element type), case-insensitive.
        /// </summary>
        public static bool IsAllowed(string? propertyPath, Type viewType)
        {
            if (string.IsNullOrWhiteSpace(propertyPath))
            {
                return false;
            }

            Type current = viewType;

            foreach (var segment in propertyPath.Split('.'))
            {
                var property = current.GetProperty(segment, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (property == null)
                {
                    return false;
                }

                current = ElementTypeOf(property.PropertyType);
            }

            return true;
        }

        private static void EnsureAllowed(string? propertyPath, Type viewType, string usage)
        {
            if (!IsAllowed(propertyPath, viewType))
            {
                throw new InvalidQueryException($"Property '{propertyPath}' cannot be used to {usage}.");
            }
        }

        private static Type ElementTypeOf(Type type)
        {
            if (type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type) && type.IsGenericType)
            {
                return type.GetGenericArguments()[0];
            }

            return Nullable.GetUnderlyingType(type) ?? type;
        }
    }
}
