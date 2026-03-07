using ApiBase.Domain.Query;
using System.Linq.Expressions;
using System.Reflection;

namespace ApiBase.Infra.Extensions
{
    /// <summary>
    /// Builds dynamic WHERE clauses from a list of FilterGroups using Expression trees.
    /// </summary>
    public class DynamicWhereBuilder
    {
        /// <summary>
        /// Applies all filter groups to the query, combining them with AND/OR logic as specified.
        /// </summary>
        public IQueryable<T> Build<T>(IQueryable<T> query, List<FilterGroup> filterGroups)
        {
            if (filterGroups == null || !filterGroups.Any())
            {
                return query;
            }

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression finalExpression = null;

            foreach (var group in filterGroups)
            {
                Expression groupExpression = null;

                foreach (var filter in group.Filters)
                {
                    var property = GetProperty(typeof(T), filter.Property, out MemberExpression memberExpression, parameter);

                    if (property == null || string.IsNullOrEmpty(filter.Value?.ToString()))
                        continue;

                    var condition = BuildCondition(filter, property, memberExpression, query);
                    
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

        private PropertyInfo GetProperty(Type type, string path, out MemberExpression memberExpr, ParameterExpression param)
        {
            memberExpr = null;
            PropertyInfo property = null;
            var segments = path.Split('.');

            foreach (var segment in segments)
            {
                property = type.GetProperty(segment, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                
                if (property == null)
                {
                    return null;
                }

                memberExpr = memberExpr == null ? Expression.Property(param, property) : Expression.Property(memberExpr, property);

                type = property.PropertyType;
            }

            return property;
        }

        private Expression BuildCondition(FilterModel filter, PropertyInfo property, MemberExpression memberExpr, IQueryable query)
        {
            object value = ValueConverter.Convert(filter, property, memberExpr);
            
            if (value == null)
            {
                return null;
            }

            var right = Expression.Constant(value, property.PropertyType);
            return FilterExpressionFactory.Create(filter, property, memberExpr, right, value, query);
        }
    }
}
