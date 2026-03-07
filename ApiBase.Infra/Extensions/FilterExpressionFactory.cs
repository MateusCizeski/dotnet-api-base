using ApiBase.Domain.Enums;
using ApiBase.Domain.Query;
using System.Linq.Expressions;
using System.Reflection;

namespace ApiBase.Infra.Extensions
{
    public static class FilterExpressionFactory
    {
        private static readonly MethodInfo StringToLower = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
        private static readonly MethodInfo StringContains = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
        private static readonly MethodInfo StringStartsWith = typeof(string).GetMethod("StartsWith", new[] { typeof(string) })!;
        private static readonly MethodInfo StringEndsWith = typeof(string).GetMethod("EndsWith", new[] { typeof(string) })!;

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
                    return Expression.Call(Expression.Call(left, StringToLower), StringContains, Expression.Call(right, StringToLower));

                case FilterOperator.StartsWith:
                    return Expression.Call(Expression.Call(left, StringToLower), StringStartsWith, Expression.Call(right, StringToLower));

                case FilterOperator.EndsWith:
                    return Expression.Call(Expression.Call(left, StringToLower), StringEndsWith, Expression.Call(right, StringToLower));

                case FilterOperator.In:
                    return Expression.Call(Expression.Constant(value), value.GetType().GetMethod("Contains", new[] { left.Type })!, left);

                case FilterOperator.InOrNull:
                    var contains = Expression.Call(Expression.Constant(value), value.GetType().GetMethod("Contains", new[] { left.Type })!, left);
                    var isNull = Expression.Equal(left, Expression.Constant(null, left.Type));
                    return Expression.OrElse(contains, isNull);

                case FilterOperator.ContainsAll:
                    if (value is IEnumerable<string> stringValues)
                    {
                        Expression combined = null;
                        foreach (var v in stringValues)
                        {
                            var item = Expression.Call(
                                Expression.Call(left, StringToLower),
                                StringContains,
                                Expression.Constant(v.ToLower())
                            );
                            combined = combined == null ? item : Expression.AndAlso(combined, item);
                        }
                        return combined ?? Expression.Constant(true);
                    }
                    throw new NotSupportedException($"ContainsAll requires a string collection value.");

                default:
                    throw new NotSupportedException($"Operator '{filter.GetOperator()}' is not supported.");
            }
        }
    }
}
