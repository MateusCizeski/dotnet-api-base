using ApiBase.Infra.Resolvers;
using System.Linq.Expressions;

namespace ApiBase.Infra.Projection
{
    /// <summary>
    /// Provides a fluent API for projecting an <see cref="IQueryable{TSource}"/> to a destination type
    /// using automatically built expression tree selectors.
    /// Obtain an instance via the <c>.Project()</c> extension method on any queryable.
    /// </summary>
    /// <typeparam name="TSource">The source entity or object type.</typeparam>
    public class Projection<TSource> where TSource : class
    {
        protected readonly IQueryable<TSource> QueryableSource;

        public Projection(IQueryable<TSource> source)
        {
            QueryableSource = source;
        }

        /// <summary>
        /// Projects the source queryable to the specified destination type.
        /// </summary>
        public IQueryable<TDestination> To<TDestination>() where TDestination : new()
        {
            var selector = new ProjectionBuilder().Build<TSource, TDestination>();
            return QueryableSource.Select(selector);
        }

        /// <summary>
        /// Projects the source queryable to a dynamically specified target type.
        /// Typically used with types built by <see cref="ApiBase.Infra.Extensions.CustomTypeBuilder"/>.
        /// </summary>
        public IQueryable<object> To(Type targetType)
        {
            var selector = new ProjectionBuilder().Build<TSource>(targetType);
            return QueryableSource.Select(selector);
        }

        /// <summary>
        /// Returns the projection as a compiled expression for use outside of IQueryable.
        /// </summary>
        public Expression<Func<TSource, TDestination>> Expression<TDestination>() where TDestination : new() => new ProjectionBuilder().Build<TSource, TDestination>();

        /// <summary>
        /// Returns the projection as a compiled delegate for in-memory use.
        /// </summary>
        public Func<TSource, TDestination> Compile<TDestination>() where TDestination : new() => Expression<TDestination>().Compile();
    }

    /// <summary>
    /// Typed projection for reference types. Extends <see cref="Projection{TSource}"/>
    /// with an additional strongly-typed <c>To&lt;TDestination&gt;()</c> overload
    /// that does not require the <c>new()</c> constraint.
    /// </summary>
    public class ObjectProjection<TSource> : Projection<TSource> where TSource : class
    {
        public ObjectProjection(IQueryable<TSource> source) : base(source) { }

        /// <summary>
        /// Projects the source queryable to the specified destination type.
        /// Does not require a parameterless constructor on the destination type.
        /// </summary>
        public new IQueryable<TDestination> To<TDestination>()
        {
            var selector = new ProjectionBuilder().Build<TSource, TDestination>();
            return QueryableSource.Select(selector);
        }
    }

    /// <summary>
    /// Builds strongly-typed and dynamically-typed projection expressions
    /// from a source type to a destination type using <see cref="MemberInitResolver"/>.
    /// </summary>
    public class ProjectionBuilder
    {
        /// <summary>
        /// Builds a strongly-typed projection expression from <typeparamref name="TSource"/> to <typeparamref name="TDestination"/>.
        /// </summary>
        public Expression<Func<TSource, TDestination>> Build<TSource, TDestination>()
        {
            var sourceType = typeof(TSource);
            var parameter = Expression.Parameter(sourceType, "p");
            return Expression.Lambda<Func<TSource, TDestination>>(new MemberInitResolver().Resolve(0, parameter, sourceType, typeof(TDestination)), parameter);
        }

        /// <summary>
        /// Builds a projection expression from <typeparamref name="TSource"/> to a dynamically specified target type.
        /// Returns <see cref="Expression{Func}"/> typed as <c>Func&lt;TSource, object&gt;</c>.
        /// </summary>
        public Expression<Func<TSource, object>> Build<TSource>(Type targetType)
        {
            var sourceType = typeof(TSource);
            var parameter = Expression.Parameter(sourceType, "p");
            return Expression.Lambda<Func<TSource, object>>(new MemberInitResolver().Resolve(0, parameter, sourceType, targetType), parameter);
        }
    }
}
