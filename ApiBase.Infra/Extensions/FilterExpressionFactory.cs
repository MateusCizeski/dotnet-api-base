using ApiBase.Domain.Enums;
using ApiBase.Domain.Query;
using System.Linq.Expressions;
using System.Reflection;

namespace ApiBase.Infra.Extensions
{
    /// <summary>
    /// Builds LINQ expression tree conditions from a <see cref="FilterModel"/>.
    /// Each operator produces a composable <see cref="Expression"/> that can be
    /// combined via <c>AndAlso</c> / <c>OrElse</c> in the query pipeline.
    /// </summary>
    public static class FilterExpressionFactory
    {
        private static readonly MethodInfo StringToLower = typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!;
        private static readonly MethodInfo StringContains = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!;
        private static readonly MethodInfo StringStartsWith = typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) })!;
        private static readonly MethodInfo StringEndsWith = typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) })!;

        /// <summary>
        /// Creates a filter expression for the given operator.
        /// </summary>
        /// <param name="filter">The filter model containing the operator and value.</param>
        /// <param name="property">The entity property being filtered.</param>
        /// <param name="left">Member expression for the property (left side of the comparison).</param>
        /// <param name="right">Constant expression for the converted value (right side).</param>
        /// <param name="value">The already-converted value object.</param>
        /// <param name="query">The source queryable (used for type inference when needed).</param>
        public static Expression Create(FilterModel filter, PropertyInfo property, Expression left, Expression right, object value, IQueryable query)
        {
            switch (filter.GetOperator())
            {
                case FilterOperator.Equal:
                    return Expression.Equal(left, right);

                case FilterOperator.NotEqual:
                    return Expression.NotEqual(left, right);

                case FilterOperator.GreaterThan:
                    return Expression.GreaterThan(left, right);

                case FilterOperator.LessThan:
                    return Expression.LessThan(left, right);

                case FilterOperator.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(left, right);

                case FilterOperator.LessThanOrEqual:
                    return Expression.LessThanOrEqual(left, right);

                case FilterOperator.Contains:
                    return BuildStringOp(left, right, StringContains);

                case FilterOperator.StartsWith:
                    return BuildStringOp(left, right, StringStartsWith);

                case FilterOperator.EndsWith:
                    return BuildStringOp(left, right, StringEndsWith);

                case FilterOperator.In:
                    {
                        var containsMethod = value.GetType().GetMethod("Contains", new[] { left.Type })!;
                        return Expression.Call(Expression.Constant(value), containsMethod, left);
                    }

                case FilterOperator.InOrNull:
                    {
                        var containsMethod = value.GetType().GetMethod("Contains", new[] { left.Type })!;
                        var containsExpr = Expression.Call(Expression.Constant(value), containsMethod, left);
                        var isNullExpr = Expression.Equal(left, Expression.Constant(null, left.Type));
                        return Expression.OrElse(containsExpr, isNullExpr);
                    }

                case FilterOperator.ContainsAll:
                    if (value is IEnumerable<string> stringValues)
                    {
                        Expression? combined = null;
                        foreach (var term in stringValues)
                        {
                            var item = BuildStringOp(left, Expression.Constant(term), StringContains);
                            combined = combined == null ? item : Expression.AndAlso(combined, item);
                        }
                        return combined ?? Expression.Constant(true);
                    }
                    throw new NotSupportedException("ContainsAll requires a string collection value.");

                default:
                    throw new NotSupportedException($"Operator '{filter.GetOperator()}' is not supported.");
            }
        }

        /// <summary>
        /// Builds a case-insensitive string comparison (Contains, StartsWith, EndsWith).
        /// Both sides are lowercased. Null-safe: short-circuits to false if left is null.
        /// </summary>
        private static Expression BuildStringOp(Expression left, Expression right, MethodInfo method)
        {
            var nullCheck = Expression.NotEqual(left, Expression.Constant(null, left.Type));
            var leftLower = Expression.Call(left, StringToLower);

            Expression rightLower;

            if (right.NodeType == ExpressionType.Constant)
            {
                rightLower = Expression.Constant(((ConstantExpression)right).Value?.ToString()?.ToLower(), typeof(string));
            }
            else
            {
                rightLower = Expression.Call(right, StringToLower);
            }

            var comparison = Expression.Call(leftLower, method, rightLower);
            return Expression.AndAlso(nullCheck, comparison);
        }
    }
}
