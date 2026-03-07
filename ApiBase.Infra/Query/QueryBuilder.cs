using ApiBase.Domain.Query;
using ApiBase.Infra.Extensions;

namespace ApiBase.Infra.Query
{
    /// <summary>
    /// Combines filter and sort operations into a single query pipeline step.
    /// </summary>
    public class QueryBuilder<T>
    {
        /// <summary>
        /// Applies filters and sorting to the query, returning the composed IQueryable directly.
        /// </summary>
        public IQueryable<T> Build(IQueryable<T> query, List<FilterGroup> filters, List<SortModel> sorters)
        {
            query = new DynamicWhereBuilder().Build(query, filters);
            query = new OrderByQuery().ApplySorting(query, sorters);

            return query;
        }
    }
}
