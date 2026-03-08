using ApiBase.Domain.Query;
using System.Collections;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ApiBase.Infra.Extensions
{
    /// <summary>
    /// Converts filter values to the target property type for use in expression trees.
    /// All numeric conversions use <see cref="CultureInfo.InvariantCulture"/> to ensure
    /// consistent parsing regardless of the server's regional settings.
    /// </summary>
    public static class ValueConverter
    {
        /// <summary>
        /// Converts the filter's Value to the type expected by the target property.
        /// Returns null if conversion fails or value is null.
        /// </summary>
        public static object? Convert(FilterModel filter, PropertyInfo property, MemberExpression? memberExpr)
        {
            var value = filter.Value;

            if (value == null) return null;

            try
            {
                var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                if (value is IList list)
                {
                    return ConvertList(list, targetType);
                }

                return ConvertScalar(value, targetType);
            }
            catch
            {
                return null;
            }
        }

        private static object? ConvertScalar(object value, Type targetType)
        {
            if (targetType == typeof(Guid))
            {
                return Guid.TryParse(value.ToString(), out var guid) ? guid : (object?)null;
            }

            if (targetType == typeof(DateTime))
            {
                return DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : (object?)null;
            }

            if (targetType == typeof(bool))
            {
                return bool.TryParse(value.ToString(), out var b) ? b : (object?)null;
            }

            if (targetType.IsEnum)
            {
                return Enum.Parse(targetType, value.ToString()!, ignoreCase: true);
            }

            if (targetType == typeof(string))
            {
                return value.ToString();
            }

            return System.Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }

        private static object ConvertList(IList list, Type targetType)
        {
            if (targetType == typeof(Guid))
            {
                return list.Cast<object>().Select(x => Guid.Parse(x.ToString()!)).ToList();
            }

            if (targetType == typeof(string))
            {
                return list.Cast<object>().Select(x => x?.ToString() ?? string.Empty).ToList();
            }

            return list.Cast<object>().Select(x => System.Convert.ChangeType(x, targetType, CultureInfo.InvariantCulture)).ToList();
        }
    }
}