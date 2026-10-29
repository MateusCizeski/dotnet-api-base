using ApiBase.Infra.Projection;

namespace ApiBase.Infra.Extensions
{
    public static class ProjectionExtensions
    {
        public static ObjectProjection<TSource> Project<TSource>(this IQueryable<TSource> source) where TSource : class
            => new ObjectProjection<TSource>(source);

        public static ObjectProjection<TSource> Project<TSource>(this IEnumerable<TSource> source) where TSource : class
            => new ObjectProjection<TSource>(source.AsQueryable());

        public static ObjectProjection<TSource>? Project<TSource>(this TSource? source) where TSource : class
            => source == null ? null : new ObjectProjection<TSource>(new List<TSource> { source }.AsQueryable());
    }
}
