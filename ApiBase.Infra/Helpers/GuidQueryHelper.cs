using ApiBase.Domain.Entities;
using ApiBase.Domain.Query;
using ApiBase.Domain.View;
using ApiBase.Infra.Extensions;
using ApiBase.Infra.Options;
using ApiBase.Infra.Query;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ApiBase.Infra.Helpers
{
    /// <summary>
    /// Runs the read pipeline used by the list endpoints:
    /// validate (allow list) → filter and sort on the entity → project to the view →
    /// shape the requested fields → count → paginate.
    /// </summary>
    public class GuidQueryHelper
    {
        private readonly ApiBaseOptions _options;

        public GuidQueryHelper(ApiBaseOptions? options = null)
        {
            _options = options ?? new ApiBaseOptions();
        }

        /// <summary>
        /// Executes the full pipeline synchronously. Prefer <see cref="PageAsync{T, TView}"/> in web requests.
        /// </summary>
        public GetView Page<T, TView>(IQueryable<T> query, QueryParams queryParams) where T : EntityGuid where TView : IdGuidView, new()
        {
            var (filtered, shaped) = BuildViewPipeline<T, TView>(query, queryParams);

            return new GetView
            {
                Total = filtered.Count(),
                Content = ApplyPagination(shaped, queryParams).ToList()
            };
        }

        /// <summary>
        /// Executes the full pipeline asynchronously (CountAsync / ToListAsync on the EF Core provider).
        /// </summary>
        public async Task<GetView> PageAsync<T, TView>(IQueryable<T> query, QueryParams queryParams, CancellationToken cancellationToken = default) where T : EntityGuid where TView : IdGuidView, new()
        {
            var (filtered, shaped) = BuildViewPipeline<T, TView>(query, queryParams);

            int total = await filtered.CountAsync(cancellationToken);
            List<object> content = await ApplyPagination(shaped, queryParams).ToListAsync(cancellationToken);

            return new GetView
            {
                Total = total,
                Content = content
            };
        }

        /// <summary>
        /// Executes a query pipeline without view projection (returns T directly).
        /// No allow list is applied: use it only with queryables whose properties are all safe to expose.
        /// </summary>
        public GetView Page<T>(IQueryable<T> query, QueryParams queryParams) where T : class => Page(query, queryParams, null);

        /// <summary>
        /// Executes a query pipeline with an optional additional predicate.
        /// No allow list is applied: use it only with queryables whose properties are all safe to expose.
        /// </summary>
        public GetView Page<T>(IQueryable<T> query, QueryParams queryParams, Expression<Func<T, bool>>? predicate) where T : class
        {
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            query = ApplyQuery(query, queryParams);
            IQueryable<object> shaped = ApplyFields(query, queryParams);

            return new GetView
            {
                Total = query.Count(),
                Content = ApplyPagination(shaped, queryParams).ToList()
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
        /// Applies Skip/Take based on Page (1-based) or Start (0-based offset) and Limit.
        /// Limit is clamped to <see cref="ApiBaseOptions.MaxPageSize"/>.
        /// </summary>
        public IQueryable<T> ApplyPagination<T>(IQueryable<T> query, QueryParams queryParams)
        {
            int limit = _options.ResolvePageSize(queryParams.Limit);
            long offset = queryParams.Page.HasValue || !queryParams.Start.HasValue
                ? (long)(Math.Max(1, queryParams.Page ?? 1) - 1) * limit
                : Math.Max(0, queryParams.Start.Value);

            return query.Skip((int)Math.Min(offset, int.MaxValue)).Take(limit);
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
        /// A stable order is required for pagination to return consistent pages.
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

        private (IQueryable<T> Filtered, IQueryable<object> Shaped) BuildViewPipeline<T, TView>(IQueryable<T> query, QueryParams queryParams) where T : EntityGuid where TView : IdGuidView, new()
        {
            QueryAllowList.Validate(queryParams.GetFilters(), queryParams.GetSort(), typeof(TView));

            IQueryable<T> filtered = ApplyQuery(query, queryParams);
            IQueryable<TView> projected = filtered.Project().To<TView>();
            IQueryable<object> shaped = ApplyFields(projected, queryParams);

            return (filtered, shaped);
        }
    }
}
