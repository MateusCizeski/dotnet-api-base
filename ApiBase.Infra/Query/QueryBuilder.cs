using ApiBase.Domain.Query;
using ApiBase.Infra.Extensions;

namespace ApiBase.Infra.Query
{
    public class QueryBuilder<T>
    {
        public IQueryable<T> Build(IQueryable<T> query, List<FilterGroup> filters, List<SortModel> sorters)
        {
            query = new DynamicWhereBuilder().Build(query, filters);
            query = new OrderByQuery().ApplySorting(query, sorters);
            return query;
        }
    }
}
