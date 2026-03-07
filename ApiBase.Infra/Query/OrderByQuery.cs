using ApiBase.Domain.Query;
using System.Linq.Expressions;

namespace ApiBase.Infra.Query
{
    public class OrderByQuery
    {
        public IQueryable<T> ApplySorting<T>(IQueryable<T> query, List<SortModel> sortModels)
        {
            if (sortModels == null || !sortModels.Any())
            {
                return query;
            }

            foreach (var sortModel in sortModels)
            {
                var methodPrefix = query.Expression.Type != typeof(IOrderedQueryable<T>) ? "Order" : "Then";
                var methodSuffix = sortModel.ASC() ? "By" : "ByDescending";
                var methodName = $"{methodPrefix}{methodSuffix}";

                var hasFilter = sortModel.FilterValue switch
                {
                    null => false, string s => !string.IsNullOrEmpty(s), _ => true
                };

                var orderedQuery = hasFilter
                    ? ApplyConditionalOrder(query, sortModel.Property, sortModel.FilterValue, methodName)
                    : ApplySimpleOrder(query, sortModel.Property, methodName);

                if (orderedQuery != null)
                {
                    query = orderedQuery;
                }
            }

            return query;
        }

        private IOrderedQueryable<T> ApplySimpleOrder<T>(IQueryable<T> source, string propertyPath, string methodName)
        {
            var propertyType = typeof(T);
            var parameter = Expression.Parameter(propertyType, "x");
            var expression = BuildPropertyExpression(propertyPath, ref propertyType, parameter);

            var lambda = Expression.Lambda(typeof(Func<,>).MakeGenericType(typeof(T), propertyType), expression, parameter);

            return InvokeOrderMethod<T>(methodName, propertyType, source, lambda);
        }

        private IOrderedQueryable<T> ApplyConditionalOrder<T>(IQueryable<T> source, string propertyPath, object filterValue, string methodName)
        {
            var propertyType = typeof(T);
            var parameter = Expression.Parameter(propertyType, "x");
            var expression = BuildPropertyExpression(propertyPath, ref propertyType, parameter);

            var convertedFilter = ConvertFilterValue(filterValue, propertyType);

            if (convertedFilter == null)
            {
                return ApplySimpleOrder(source, propertyPath, methodName);
            }

            if (propertyType == typeof(string))
                expression = Expression.Call(expression, "ToLower", null);

            var condition = Expression.Condition(
                Expression.Equal(expression, Expression.Constant(convertedFilter)),
                Expression.Constant(0),
                Expression.Constant(1),
                typeof(int)
            );

            var lambda = Expression.Lambda<Func<T, int>>(condition, parameter);
            return InvokeOrderMethod<T>(methodName, typeof(int), source, lambda);
        }

        private Expression BuildPropertyExpression(string propertyPath, ref Type type, ParameterExpression parameter)
        {
            Expression expression = parameter;

            foreach (var prop in propertyPath.Split('.'))
            {
                var propertyInfo = type.GetProperty(prop);

                if (propertyInfo == null)
                    throw new ArgumentException($"Property '{prop}' not found on type '{type.Name}'");

                expression = Expression.Property(expression, propertyInfo);
                type = propertyInfo.PropertyType;
            }

            return expression;
        }

        private IOrderedQueryable<T> InvokeOrderMethod<T>(string methodName, Type propertyType, IQueryable<T> source, LambdaExpression lambda)
        {
            var method = typeof(Queryable).GetMethods()
                .Single(m => m.Name == methodName &&
                             m.IsGenericMethodDefinition &&
                             m.GetGenericArguments().Length == 2 &&
                             m.GetParameters().Length == 2);

            var genericMethod = method.MakeGenericMethod(typeof(T), propertyType);
            return (IOrderedQueryable<T>)genericMethod.Invoke(null, new object[] { source, lambda });
        }

        private object ConvertFilterValue(object filterValue, Type propertyType)
        {
            try
            {
                if (propertyType == typeof(long) || propertyType == typeof(long?))
                {
                    return Convert.ToInt64(filterValue);
                }

                if (propertyType == typeof(int) || propertyType == typeof(int?))
                {
                    return Convert.ToInt32(filterValue);
                }

                if (propertyType == typeof(string))
                {
                    return filterValue.ToString()?.ToLower();
                }

                return filterValue;
            }
            catch
            {
                return null;
            }
        }
    }
}
