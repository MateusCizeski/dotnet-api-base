using ApiBase.Domain.Query;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace ApiBase.Infra.Extensions
{
    /// <summary>
    /// Converts filter values to the target property type for use in expression trees.
    /// </summary>
    public static class ValueConverter
    {
        /// <summary>
        /// Converts the filter's Value to the type expected by the target property.
        /// Returns null if conversion fails or value is null.
        /// </summary>
        public static object Convert(FilterModel filter, PropertyInfo property, MemberExpression memberExpr)
        {
            // PascalCase after Domain rename
            var value = filter.Value;
            if (value == null) return null;

            try
            {
                var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                if (value is IList list)
                {
                    if (targetType == typeof(Guid))
                        return list.Cast<object>().Select(x => Guid.Parse(x.ToString())).ToList();

                    return list.Cast<object>().Select(x => System.Convert.ChangeType(x, targetType)).ToList();
                }

                if (targetType == typeof(Guid))
                    return Guid.TryParse(value.ToString(), out var guid) ? guid : null;

                if (targetType == typeof(DateTime))
                    return DateTime.TryParse(value.ToString(), out var date) ? date : null;

                if (targetType == typeof(bool))
                    return bool.TryParse(value.ToString(), out var b) ? b : null;

                if (targetType.IsEnum)
                    return Enum.Parse(targetType, value.ToString(), ignoreCase: true);

                if (targetType == typeof(string))
                    return value.ToString();

                return System.Convert.ChangeType(value, targetType);
            }
            catch
            {
                return null;
            }
        }
    }
}
