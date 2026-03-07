using ApiBase.Domain.Entities;
using ApiBase.Domain.Query;
using ApiBase.Domain.View;
using ApiBase.Infra.Extensions;
using ApiBase.Infra.Query;
using System.Linq.Expressions;

namespace ApiBase.Infra.Helpers
{
    public class GuidQueryHelper
    {
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

        public GetView Page<T>(IQueryable<T> query, QueryParams queryParams) where T : class
        {
            return Page(query, queryParams, null);
        }

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

        public IQueryable<T> ApplyQuery<T>(IQueryable<T> query, QueryParams queryParams) where T : class
        {
            var sorting = BuildOrderBy<T>(queryParams);
            var filters = queryParams.GetFilters();

            return new QueryBuilder<T>().Build(query, filters, sorting);
        }

        public List<T> ExecutePagination<T>(IQueryable<T> query, QueryParams queryParams)
        {
            int page = Math.Max(1, queryParams.page.GetValueOrDefault(1));
            int limit = Math.Max(1, queryParams.limit.GetValueOrDefault(25));

            return query.Skip((page - 1) * limit).Take(limit).ToList();
        }

        public IQueryable<T> OrderBy<T>(IQueryable<T> query, QueryParams queryParams) where T : class
        {
            var sortList = BuildOrderBy<T>(queryParams);
            return new OrderByQuery().ApplySorting(query, sortList);
        }

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

        public IQueryable<object> ApplyFields<T>(IQueryable<T> query, QueryParams queryParams) where T : class
        {
            IQueryable<object> result = query;
            List<string> fields = queryParams.GetFields();

            if (fields.Any())
            {
                result = query.SelectDynamic(fields);
            }

            return result;
        }
    }
}
