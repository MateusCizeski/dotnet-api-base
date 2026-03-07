using ApiBase.Domain.Entities;
using ApiBase.Domain.Query;
using ApiBase.Domain.View;
using ApiBase.Infra.Extensions;
using ApiBase.Infra.Query;
using System.Linq.Expressions;

namespace ApiBase.Infra.Helpers
{
    /// <summary>
    /// Provides query execution helpers for paginated, filtered and projected results
    /// over entities that implement <see cref="EntityGuid"/>.
    /// </summary>
    public class GuidQueryHelper
    {
        /// <summary>
        /// Executes a full query pipeline: filter → project → shape fields → paginate.
        /// </summary>
        public GetView Page<T, TView>(IQueryable<T> query, QueryParams queryParams) where T : EntityGuid where TView : IdGuidView, new()
        {
            IQueryable<T> filtered = ApplyQuery(query, queryParams);
            IQueryable<TView> projected = filtered.Project().To<TView>();
            IQueryable<object> shaped = ApplyFields(projected, queryParams);

            return new GetView
            {
                Total = shaped.Count(),
                Content = ExecutePagination(shaped, queryParams)
            };
        }

        /// <summary>
        /// Executes a query pipeline without view projection (returns T directly).
        /// </summary>
        public GetView Page<T>(IQueryable<T> query, QueryParams queryParams) where T : class => Page(query, queryParams, null);

        /// <summary>
        /// Executes a query pipeline with an optional additional predicate.
        /// </summary>
        public GetView Page<T>(IQueryable<T> query, QueryParams queryParams, Expression<Func<T, bool>> predicate) where T : class
        {
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            query = ApplyQuery(query, queryParams);
            IQueryable<object> shaped = ApplyFields(query, queryParams);

            return new GetView
            {
                Total = shaped.Count(),
                Content = ExecutePagination(shaped, queryParams)
            };
        }

        /// <summary>
        /// Applies filters and sorting from <paramref name="queryParams"/> to the query.
        /// </summary>
        public IQueryable<T> ApplyQuery<T>(IQueryable<T> query, QueryParams queryParams) where T : class
        {
            var sorting = BuildOrderBy<T>(queryParams);
            var filters = queryParams.GetFilters();

            return new QueryBuilder<T>().Build(query, filters, sorting);
        }

        /// <summary>
        /// Executes pagination based on Page and Limit from <paramref name="queryParams"/>.
        /// </summary>
        public List<T> ExecutePagination<T>(IQueryable<T> query, QueryParams queryParams)
        {
            int page = Math.Max(1, queryParams.Page.GetValueOrDefault(1));
            int limit = Math.Max(1, queryParams.Limit.GetValueOrDefault(25));
            return query.Skip((page - 1) * limit).Take(limit).ToList();
        }

        /// <summary>
        /// Applies only the ordering rules from <paramref name="queryParams"/> to the query.
        /// </summary>
        public IQueryable<T> OrderBy<T>(IQueryable<T> query, QueryParams queryParams) where T : class
        {
            var sortList = BuildOrderBy<T>(queryParams);
            return new OrderByQuery().ApplySorting(query, sortList);
        }

        /// <summary>
        /// Resolves the sort list from query params, defaulting to ascending Id if none provided.
        /// </summary>
        public List<SortModel> BuildOrderBy<T>(QueryParams queryParams)
        {
            var sortList = queryParams.GetSort();
            
            if (sortList != null && sortList.Count > 0)
            {
                return sortList;
            }

            if (typeof(T).GetProperty("Id") != null)
            {
                return new List<SortModel>
                {
                    new SortModel { Property = "Id", Direction = "asc" }
                };
            }

            return new List<SortModel>();
        }

        /// <summary>
        /// Applies dynamic field projection if specific fields are requested.
        /// </summary>
        public IQueryable<object> ApplyFields<T>(IQueryable<T> query, QueryParams queryParams) where T : class
        {
            var fields = queryParams.GetFields();

            if (fields.Any())
            {
                return query.SelectDynamic(fields);
            }

            return query.Cast<object>();
        }
    }
}
