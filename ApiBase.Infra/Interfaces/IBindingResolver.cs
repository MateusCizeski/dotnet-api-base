using ApiBase.Infra.Resolvers;
using System.Linq.Expressions;
using System.Reflection;

namespace ApiBase.Infra.Interfaces
{
    /// <summary>
    /// Resolves a single property binding expression when building a projection from source to destination type.
    /// Each implementation handles a specific binding strategy (direct, conversion, association, list, complex).
    /// </summary>
    public interface IBindingResolver
    {
        /// <summary>
        /// Builds a <see cref="MemberAssignment"/> that maps <paramref name="sourceProperty"/>
        /// to <paramref name="destinationProperty"/> in the projection expression tree.
        /// </summary>
        /// <param name="memberInitResolver">The parent resolver, used for recursive nested type resolution.</param>
        /// <param name="level">Current nesting depth, used to generate unique parameter names.</param>
        /// <param name="parentExpression">The expression representing the source object instance.</param>
        /// <param name="sourceProperty">The property on the source type.</param>
        /// <param name="destinationProperty">The property on the destination type.</param>
        MemberAssignment Resolve(MemberInitResolver memberInitResolver, int level, Expression parentExpression, PropertyInfo sourceProperty, PropertyInfo destinationProperty);
    }
}
