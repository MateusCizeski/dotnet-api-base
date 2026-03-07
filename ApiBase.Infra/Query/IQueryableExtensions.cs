using ApiBase.Infra.Extensions;
using System.Linq.Expressions;
using System.Reflection;

namespace ApiBase.Infra.Query
{
    public static class IQueryableExtensions
    {
        public static IQueryable<dynamic> SelectDynamic<T>(this IQueryable<T> source, IEnumerable<string> propertyNames)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var selector = BuildSelector(typeof(T), propertyNames);

            return source.Select(selector as Expression<Func<T, object>>);
        }

        public static IQueryable<dynamic> SelectDynamic<T>(this IQueryable<T> source, string propertyNames)
        {
            var properties = propertyNames.Split(',').Select(p => p.Trim()).Where(p => !string.IsNullOrWhiteSpace(p));

            return source.SelectDynamic(properties);
        }

        private static LambdaExpression BuildSelector(Type sourceType, IEnumerable<string> propertyNames)
        {
            var param = Expression.Parameter(sourceType, $"t_{Guid.NewGuid():N}");
            var bindings = new List<MemberBinding>();
            var dynamicProperties = new Dictionary<string, Type>();

            foreach (var name in propertyNames)
            {
                var propertyInfo = GetPropertyByPath(sourceType, name);
                if (propertyInfo == null) continue;

                dynamicProperties[name.Replace(".", "_")] = propertyInfo.PropertyType;
            }

            var dynamicType = DynamicTypeBuilderExtension.FromPropertyDictionary(dynamicProperties);

            foreach (var kvp in dynamicProperties)
            {
                var memberAccess = BuildMemberAccess(param, kvp.Key.Replace("_", "."));
                bindings.Add(Expression.Bind(dynamicType.GetProperty(kvp.Key), memberAccess));
            }

            var initializer = Expression.MemberInit(Expression.New(dynamicType), bindings);
            var delegateType = typeof(Func<,>).MakeGenericType(sourceType, typeof(object));

            return Expression.Lambda(delegateType, initializer, param);
        }

        private static MemberExpression BuildMemberAccess(Expression param, string propertyPath)
        {
            var parts = propertyPath.Split('.');
            Expression body = param;

            foreach (var part in parts)
            {
                var prop = body.Type.GetProperty(part);

                if (prop == null)
                {
                    throw new InvalidOperationException($"Property '{part}' not found on type '{body.Type.Name}'.");
                }

                body = Expression.Property(body, prop);
            }

            return (MemberExpression)body;
        }

        private static PropertyInfo GetPropertyByPath(Type baseType, string path)
        {
            var parts = path.Split('.');
            var currentType = baseType;
            PropertyInfo property = null;

            foreach (var part in parts)
            {
                property = currentType.GetProperty(part);

                if (property == null)
                {
                    return null;
                }

                currentType = property.PropertyType;
            }

            return property;
        }
    }
}
