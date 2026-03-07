using ApiBase.Infra.Interfaces;
using System.Collections;
using System.Reflection;

namespace ApiBase.Infra.Bindings
{
    /// <summary>
    /// Selects and caches the appropriate <see cref="IBindingResolver"/> for a given source/destination property pair.
    /// Resolver selection follows this priority:
    /// <list type="number">
    ///   <item>Same type → <see cref="DefaultBindingResolver"/></item>
    ///   <item>IEnumerable source → <see cref="ListBindingResolver"/></item>
    ///   <item>Complex class with <see cref="ComplexBindingResolver"/> attribute → <see cref="ComplexBindingResolver"/></item>
    ///   <item>Complex class → <see cref="AssociationBindingResolver"/></item>
    ///   <item>Different primitive/value types → <see cref="ConversionBindingResolver"/></item>
    /// </list>
    /// </summary>
    public class BindingFactory
    {
        private readonly Dictionary<Type, IBindingResolver> _resolverCache = new();

        private IBindingResolver GetResolver(Type resolverType)
        {
            if (_resolverCache.TryGetValue(resolverType, out var resolver))
            {
                return resolver;
            }

            var instance = (IBindingResolver)Activator.CreateInstance(resolverType)!;
            _resolverCache[resolverType] = instance;
            return instance;
        }

        /// <summary>
        /// Returns the most appropriate <see cref="IBindingResolver"/> for the given property pair.
        /// </summary>
        public IBindingResolver GetInstance(PropertyInfo sourceProperty, PropertyInfo destinationProperty)
        {
            var sourceType = sourceProperty.PropertyType;
            var destinationType = destinationProperty.PropertyType;

            if (sourceType == destinationType)
            {
                return GetResolver(typeof(DefaultBindingResolver));
            }

            if (sourceType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(sourceType))
            {
                return GetResolver(typeof(ListBindingResolver));
            }

            if (destinationType.IsClass)
            {
                if (Attribute.IsDefined(sourceProperty, typeof(ComplexBindingResolver)) || Attribute.IsDefined(destinationProperty, typeof(ComplexBindingResolver)))
                {
                    return GetResolver(typeof(ComplexBindingResolver));
                }

                return GetResolver(typeof(AssociationBindingResolver));
            }

            return GetResolver(typeof(ConversionBindingResolver));
        }
    }
}
