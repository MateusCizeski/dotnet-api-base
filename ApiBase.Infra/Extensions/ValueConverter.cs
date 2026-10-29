using ApiBase.Domain.Exceptions;
using ApiBase.Domain.Query;
using System.Collections;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

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
        /// Returns null only when the value itself is null.
        /// </summary>
        /// <exception cref="InvalidQueryException">The value cannot be converted to the property type.</exception>
        public static object? Convert(FilterModel filter, PropertyInfo property, MemberExpression? memberExpr)
        {
            var value = Unwrap(filter.Value);

            if (value == null)
            {
                return null;
            }

            var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            try
            {
                if (value is IList list)
                {
                    return ConvertList(list, targetType, property.PropertyType);
                }

                return ConvertScalar(value, targetType);
            }
            catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException or ArgumentException)
            {
                throw InvalidValue(filter, ex);
            }
        }

        /// <summary>
        /// Values deserialized with System.Text.Json arrive as <see cref="JsonElement"/>.
        /// Arrays become a list (needed by In / InOrNull / ContainsAll); scalars become their text.
        /// </summary>
        private static object? Unwrap(object? value)
        {
            if (value is not JsonElement element)
            {
                return value;
            }

            return element.ValueKind switch
            {
                JsonValueKind.Null or JsonValueKind.Undefined => null,
                JsonValueKind.Array => element.EnumerateArray().Select(item => Unwrap(item)).ToList(),
                JsonValueKind.String => element.GetString(),
                _ => element.GetRawText()
            };
        }

        private static object ConvertScalar(object value, Type targetType)
        {
            string text = value.ToString() ?? string.Empty;

            if (targetType == typeof(Guid))
            {
                return Guid.Parse(text);
            }

            if (targetType == typeof(DateTime))
            {
                return DateTime.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.None);
            }

            if (targetType == typeof(DateTimeOffset))
            {
                return DateTimeOffset.Parse(text, CultureInfo.InvariantCulture);
            }

            if (targetType == typeof(bool))
            {
                return bool.Parse(text);
            }

            if (targetType.IsEnum)
            {
                return Enum.Parse(targetType, text, ignoreCase: true);
            }

            if (targetType == typeof(string))
            {
                return text;
            }

            return System.Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Builds a List typed with the property type (List&lt;int?&gt; for an int? property),
        /// so the expression list.Contains(x.Property) finds the right Contains overload.
        /// </summary>
        private static object ConvertList(IList list, Type targetType, Type elementType)
        {
            var listType = typeof(List<>).MakeGenericType(elementType);
            var result = (IList)Activator.CreateInstance(listType)!;

            foreach (var item in list)
            {
                if (item == null)
                {
                    continue;
                }

                result.Add(ConvertScalar(item, targetType));
            }

            return result;
        }

        private static InvalidQueryException InvalidValue(FilterModel filter, Exception inner)
        {
            return new InvalidQueryException($"Value '{FormatValue(filter.Value)}' is not valid for '{filter.Property}'.", inner);
        }

        private static string FormatValue(object? value)
        {
            return value is JsonElement element ? element.GetRawText() : value?.ToString() ?? string.Empty;
        }
    }
}
