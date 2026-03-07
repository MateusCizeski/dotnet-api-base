using ApiBase.Infra.Bindings;
using System.Linq.Expressions;

namespace ApiBase.Infra.Resolvers
{
    /// <summary>
    /// Builds a <see cref="MemberInitExpression"/> that projects all writable public properties
    /// from a source type to a destination type, using the appropriate binding strategy
    /// selected by <see cref="BindingFactory"/> for each property pair.
    /// Supports recursive resolution for nested complex types and collections.
    /// </summary>
    public class MemberInitResolver
    {
        private readonly BindingFactory _bindingFactory;

        public MemberInitResolver()
        {
            _bindingFactory = new BindingFactory();
        }

        /// <summary>
        /// Builds the member initialization expression for the given source → destination type mapping.
        /// </summary>
        /// <param name="level">Current nesting depth (used for unique parameter naming in nested projections).</param>
        /// <param name="parentExpression">Expression representing the current source object instance.</param>
        /// <param name="sourceType">The type of the source object.</param>
        /// <param name="destinationType">The type to project into.</param>
        public MemberInitExpression Resolve(int level, Expression parentExpression, Type sourceType, Type destinationType)
        {
            level++;

            var destinationProperties = destinationType
                .GetProperties()
                .Where(p => p.CanWrite && p.SetMethod != null && p.SetMethod.IsPublic)
                .ToList();

            var bindings = new List<MemberBinding>();

            foreach (var destinationProperty in destinationProperties)
            {
                var sourceProperty = sourceType.GetProperty(destinationProperty.Name);

                if (sourceProperty == null)
                {
                    continue;
                }

                var assignment = _bindingFactory
                    .GetInstance(sourceProperty, destinationProperty)
                    .Resolve(this, level, parentExpression, sourceProperty, destinationProperty);

                bindings.Add(assignment);
            }

            return Expression.MemberInit(Expression.New(destinationType), bindings);
        }
    }
}
