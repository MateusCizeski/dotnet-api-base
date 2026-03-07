using ApiBase.Infra.Interfaces;
using ApiBase.Infra.Resolvers;
using System.Linq.Expressions;
using System.Reflection;

namespace ApiBase.Infra.Bindings
{
    /// <summary>
    /// Resolves property bindings for navigation/association properties (non-collection complex types).
    /// Generates a null-safe conditional expression: if source is null, bind null; otherwise project recursively.
    /// </summary>
    public class AssociationBindingResolver : IBindingResolver
    {
        /// <inheritdoc/>
        public MemberAssignment Resolve(MemberInitResolver memberInitResolver, int level, Expression parentExpression, PropertyInfo sourceProperty, PropertyInfo destinationProperty)
        {
            MemberExpression memberExpression = Expression.Property(parentExpression, sourceProperty);
            MemberInitExpression ifFalse = memberInitResolver.Resolve(level, memberExpression, sourceProperty.PropertyType, destinationProperty.PropertyType);
            ConditionalExpression expression = Expression.Condition(Expression.Equal(memberExpression, Expression.Constant(null)), Expression.Constant(null, destinationProperty.PropertyType), ifFalse);
            return Expression.Bind(destinationProperty, expression);
        }
    }

    /// <summary>
    /// Resolves property bindings for properties that require explicit type conversion
    /// (e.g. int → long, enum → int).
    /// </summary>
    public class ConversionBindingResolver : IBindingResolver
    {
        /// <inheritdoc/>
        public MemberAssignment Resolve(MemberInitResolver memberInitResolver, int level, Expression parentExpression, PropertyInfo sourceProperty, PropertyInfo destinationProperty)
        {
            UnaryExpression expression = Expression.Convert(Expression.Property(parentExpression, sourceProperty), destinationProperty.PropertyType);
            return Expression.Bind(destinationProperty, expression);
        }
    }

    /// <summary>
    /// Resolves property bindings for properties with identical source and destination types.
    /// Generates a direct property access expression with no conversion.
    /// </summary>
    public class DefaultBindingResolver : IBindingResolver
    {
        /// <inheritdoc/>
        public MemberAssignment Resolve(MemberInitResolver memberInitResolver, int level, Expression parentExpression, PropertyInfo sourceProperty, PropertyInfo destinationProperty)
        {
            return Expression.Bind(destinationProperty, Expression.Property(parentExpression, sourceProperty));
        }
    }

    /// <summary>
    /// Resolves property bindings for collection (IEnumerable) properties.
    /// Projects each element of the source collection to the destination element type recursively,
    /// producing a Select(...).ToList() expression.
    /// </summary>
    public class ListBindingResolver : IBindingResolver
    {
        private readonly MethodInfo _selectMethod;
        private readonly MethodInfo _toListMethod;

        public ListBindingResolver()
        {
            _selectMethod = typeof(Enumerable).GetMethods().First(m => m.Name == nameof(Enumerable.Select) && m.GetParameters().Length == 2);

            _toListMethod = typeof(Enumerable).GetMethods().First(m => m.Name == nameof(Enumerable.ToList) && m.GetParameters().Length == 1);
        }

        /// <inheritdoc/>
        public MemberAssignment Resolve(MemberInitResolver memberInitResolver, int level, Expression parentExpression, PropertyInfo sourceProperty, PropertyInfo destinationProperty)
        {
            Type sourceElementType = sourceProperty.PropertyType.GetGenericArguments().First();
            Type destinationElementType = destinationProperty.PropertyType.GetGenericArguments().First();

            ParameterExpression parameter = Expression.Parameter(sourceElementType, $"p{level}");
            LambdaExpression selectorLambda = Expression.Lambda(memberInitResolver.Resolve(level, parameter, sourceElementType, destinationElementType), parameter);

            MethodInfo selectGeneric = _selectMethod.MakeGenericMethod(sourceElementType, destinationElementType);
            MethodInfo toListGeneric = _toListMethod.MakeGenericMethod(destinationElementType);

            MemberExpression sourceMember = Expression.Property(parentExpression, sourceProperty);
            MethodCallExpression selectCall = Expression.Call(selectGeneric, sourceMember, selectorLambda);
            MethodCallExpression toListCall = Expression.Call(toListGeneric, selectCall);

            return Expression.Bind(destinationProperty, toListCall);
        }
    }

    /// <summary>
    /// Attribute that forces the <see cref="BindingFactory"/> to use <see cref="ComplexBindingResolver"/>
    /// for this property, bypassing the default binding strategy selection.
    /// Apply to source or destination properties that require custom complex projection logic.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ComplexBindingResolver : Attribute
    {
    }
}
