using ApiBase.Domain.Exceptions;
using ApiBase.Domain.Query;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace ApiBase.Infra.Extensions
{
    /// <summary>
    /// Builds dynamic WHERE clauses from a list of FilterGroups using Expression trees.
    /// A filter with an empty value is ignored (the client has not filled the field yet).
    /// A filter with an unknown property or an invalid value throws <see cref="InvalidQueryException"/>
    /// instead of being skipped — skipping would silently return unfiltered data.
    /// </summary>
    public class DynamicWhereBuilder
    {
        /// <summary>
        /// Applies all filter groups to the query, combining them with AND/OR logic as specified.
        /// </summary>
        public IQueryable<T> Build<T>(IQueryable<T> query, List<FilterGroup>? filterGroups)
        {
            if (filterGroups == null || !filterGroups.Any())
            {
                return query;
            }

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? finalExpression = null;

            foreach (var group in filterGroups)
            {
                Expression? groupExpression = null;

                foreach (var filter in group.Filters ?? [])
                {
                    if (IsEmpty(filter.Value))
                    {
                        continue;
                    }

                    var (property, memberExpression) = GetProperty(typeof(T), filter.Property, parameter);
                    var condition = BuildCondition(filter, property, memberExpression);

                    if (condition == null)
                    {
                        continue;
                    }

                    if (filter.Not)
                    {
                        condition = Expression.Not(condition);
                    }

                    groupExpression = groupExpression == null
                        ? condition
                        : (filter.And
                            ? Expression.AndAlso(groupExpression, condition)
                            : Expression.OrElse(groupExpression, condition));
                }

                if (groupExpression != null)
                {
                    finalExpression = finalExpression == null
                        ? groupExpression
                        : (group.And
                            ? Expression.AndAlso(finalExpression, groupExpression)
                            : Expression.OrElse(finalExpression, groupExpression));
                }
            }

            if (finalExpression != null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(finalExpression, parameter);
                query = query.Where(lambda);
            }

            return query;
        }

        private static bool IsEmpty(object? value)
        {
            return string.IsNullOrEmpty(value?.ToString());
        }

        private static (PropertyInfo Property, MemberExpression Member) GetProperty(Type type, string path, ParameterExpression param)
        {
            MemberExpression? memberExpr = null;
            PropertyInfo? property = null;

            foreach (var segment in (path ?? string.Empty).Split('.'))
            {
                property = type.GetProperty(segment, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                    ?? throw new InvalidQueryException($"Property '{path}' does not exist.");

                memberExpr = memberExpr == null ? Expression.Property(param, property) : Expression.Property(memberExpr, property);
                type = property.PropertyType;
            }

            return (property!, memberExpr!);
        }

        private static Expression? BuildCondition(FilterModel filter, PropertyInfo property, MemberExpression memberExpr)
        {
            object? value = ValueConverter.Convert(filter, property, memberExpr);

            if (value == null)
            {
                return null;
            }

            // Lists (In, InOrNull, ContainsAll) keep their own type; scalars are typed as the property.
            var right = value is IList ? Expression.Constant(value) : Expression.Constant(value, property.PropertyType);

            try
            {
                return FilterExpressionFactory.Create(filter, property, memberExpr, right, value, null);
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or NotSupportedException)
            {
                // e.g. "greater" on a string or "contains" on a number: the expression cannot be built.
                throw new InvalidQueryException($"Operator '{filter.GetOperator()}' cannot be used with '{filter.Property}'.", ex);
            }
        }
    }
}
