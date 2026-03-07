using System.Collections;
using System.Reflection;

namespace ApiBase.Infra.Extensions
{
    /// <summary>
    /// Helper methods for building dynamic types from property paths and dictionaries.
    /// Used internally by the query infrastructure to create projected types at runtime.
    /// </summary>
    public static class DynamicTypeBuilderExtension
    {
        private class PropertyNode
        {
            public string Name { get; set; } = string.Empty;
            public List<PropertyNode> Children { get; set; } = new();
            public override string ToString() => Name;
        }

        /// <summary>
        /// Creates a dynamic type from an explicit dictionary of property names and types.
        /// Delegates to <see cref="CustomTypeBuilder.CreateType"/>.
        /// </summary>
        public static Type FromPropertyDictionary(Dictionary<string, Type> properties)
        {
            return CustomTypeBuilder.CreateType(properties);
        }

        /// <summary>
        /// Creates a dynamic type from a list of dot-notation property paths on the given base type.
        /// Supports nested paths such as "Address.City", which result in nested dynamic types.
        /// </summary>
        /// <param name="baseType">The source type to reflect property types from.</param>
        /// <param name="propertyPaths">Dot-notation property paths, e.g. ["Id", "Name", "Address.City"].</param>
        public static Type FromPropertyPaths(Type baseType, IEnumerable<string> propertyPaths)
        {
            var propertyGraph = BuildPropertyGraph(propertyPaths);
            return CreateDynamicType(baseType, propertyGraph);
        }

        private static List<PropertyNode> BuildPropertyGraph(IEnumerable<string> paths)
        {
            var splitPaths = paths
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Split('.'))
                .ToList();

            return BuildNodeTree(splitPaths);
        }

        private static List<PropertyNode> BuildNodeTree(List<string[]> segments)
        {
            var grouped = segments.Where(s => s.Length > 0).GroupBy(s => s[0]);
            var result = new List<PropertyNode>();

            foreach (var group in grouped)
            {
                var node = new PropertyNode { Name = group.Key };
                var children = group.Where(s => s.Length > 1).Select(s => s.Skip(1).ToArray()).ToList();

                if (children.Any())
                {
                    node.Children = BuildNodeTree(children);
                }

                result.Add(node);
            }

            return result;
        }

        private static Type CreateDynamicType(Type baseType, List<PropertyNode> properties)
        {
            var propTypes = new Dictionary<string, Type>();

            foreach (var prop in properties)
            {
                var propInfo = baseType.GetProperty(prop.Name);
                
                if (propInfo == null)
                {
                    continue;
                }

                propTypes[prop.Name] = ResolvePropertyType(propInfo, prop.Children);
            }

            return CustomTypeBuilder.CreateType(propTypes);
        }

        private static Type ResolvePropertyType(PropertyInfo propInfo, List<PropertyNode> children)
        {
            var propType = propInfo.PropertyType;

            if (IsEnumerableButNotString(propType))
            {
                return CreateGenericListType(propType, children);
            }

            if (propType.IsClass && propType != typeof(string))
            {
                return children.Any() ? CreateDynamicType(propType, children) : propType;
            }

            return propType;
        }

        private static Type CreateGenericListType(Type listType, List<PropertyNode> children)
        {
            var elementTypes = listType.GetGenericArguments().Select(type =>
                type.IsPrimitive || type == typeof(string)
                    ? type
                    : children.Any()
                        ? CreateDynamicType(type, children)
                        : CreateTypeFromAllProperties(type)
            ).ToArray();

            return typeof(List<>).MakeGenericType(elementTypes);
        }

        private static Type CreateTypeFromAllProperties(Type type)
        {
            var props = type.GetProperties().ToDictionary(p => p.Name, p => p.PropertyType);
            return CustomTypeBuilder.CreateType(props);
        }

        private static bool IsEnumerableButNotString(Type type)
        {
            return typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string);
        }
    }
}
