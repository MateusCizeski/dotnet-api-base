using ApiBase.Application.Base;
using ApiBase.Domain.Entities;
using ApiBase.Domain.Exceptions;
using ApiBase.Domain.Interfaces;
using ApiBase.Domain.Query;
using ApiBase.Domain.View;
using ApiBase.Infra.Extensions;
using ApiBase.Infra.Helpers;
using ApiBase.Infra.Options;
using ApiBase.Infra.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;
using System.Reflection;

namespace ApiBase.Application.ApplicationGuid
{
    /// <summary>
    /// Base read-side application service for entities with a GUID primary key.
    /// Provides read operations (sync and async) with pagination, filtering, field projection,
    /// dynamic sorting and structured logging. Only properties declared in <typeparamref name="TView"/>
    /// are returned and can be used in filters and sorts.
    /// Override <see cref="DefaultFilter"/> to apply cross-cutting query filters
    /// such as tenant isolation or ownership checks.
    /// </summary>
    public abstract class ApplicationGuid<TEntity, TRepository, TView>
        : ApplicationBase<TEntity, TRepository>, IApplicationGuid<TView>
        where TEntity : EntityGuid
        where TRepository : IRepositoryBase<TEntity>
        where TView : IdGuidView, new()
    {
        private readonly GuidQueryHelper _queryHelper;

        /// <param name="unitOfWork">Unit of work (kept for services that also write).</param>
        /// <param name="repository">Repository used as the query source.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="options">
        /// Query options registered by <c>AddApiBaseCore</c>. Forward it from your constructor
        /// to apply a custom MaxPageSize; when null, the defaults are used (25 per page, 100 at most).
        /// </param>
        protected ApplicationGuid(IUnitOfWork unitOfWork, TRepository repository, ILogger logger, IOptions<ApiBaseOptions>? options = null)
            : base(unitOfWork, repository, logger)
        {
            _queryHelper = new GuidQueryHelper(options?.Value);
        }

        // -------------------------
        // Read (sync)
        // -------------------------

        /// <inheritdoc/>
        public virtual GetView Get(QueryParams queryParams)
        {
            Logger.LogDebug("Get list called for {Entity}.", typeof(TEntity).Name);

            GetView result = _queryHelper.Page<TEntity, TView>(BaseQuery(), queryParams);

            Logger.LogDebug("Get list returned {Total} record(s) for {Entity}.", result.Total, typeof(TEntity).Name);

            return result;
        }

        /// <inheritdoc/>
        public virtual TView? Get(Guid id)
        {
            Logger.LogDebug("Get by Id called for {Entity} Id={Id}.", typeof(TEntity).Name, id);

            return ById(id).Project().To<TView>().FirstOrDefault();
        }

        /// <inheritdoc/>
        public virtual object? Get(Guid id, List<string> fields)
        {
            if (fields == null || fields.Count == 0)
            {
                return Get(id);
            }

            Logger.LogDebug("Get by Id with fields called for {Entity} Id={Id}. Fields: {Fields}.",
                typeof(TEntity).Name, id, string.Join(", ", fields));

            return ById(id).Project().To(FieldsType(fields)).FirstOrDefault();
        }

        // -------------------------
        // Read (async)
        // -------------------------

        /// <inheritdoc/>
        public virtual async Task<GetView> GetAsync(QueryParams queryParams, CancellationToken cancellationToken = default)
        {
            Logger.LogDebug("GetAsync list called for {Entity}.", typeof(TEntity).Name);

            GetView result = await _queryHelper.PageAsync<TEntity, TView>(BaseQuery(), queryParams, cancellationToken);

            Logger.LogDebug("GetAsync list returned {Total} record(s) for {Entity}.", result.Total, typeof(TEntity).Name);

            return result;
        }

        /// <inheritdoc/>
        public virtual async Task<TView?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Logger.LogDebug("GetAsync by Id called for {Entity} Id={Id}.", typeof(TEntity).Name, id);

            return await ById(id).Project().To<TView>().FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<object?> GetAsync(Guid id, List<string> fields, CancellationToken cancellationToken = default)
        {
            if (fields == null || fields.Count == 0)
            {
                return await GetAsync(id, cancellationToken);
            }

            Logger.LogDebug("GetAsync by Id with fields called for {Entity} Id={Id}. Fields: {Fields}.",
                typeof(TEntity).Name, id, string.Join(", ", fields));

            return await ById(id).Project().To(FieldsType(fields)).FirstOrDefaultAsync(cancellationToken);
        }

        // -------------------------
        // Filter
        // -------------------------

        /// <summary>
        /// Global filter applied to all queries in this application service.
        /// Override to add tenant isolation, ownership or any other cross-cutting filter.
        /// Default returns all records.
        /// </summary>
        public virtual Expression<Func<TEntity, bool>> DefaultFilter()
        {
            return e => true;
        }

        // -------------------------
        // Helpers
        // -------------------------

        /// <summary>
        /// Query source for every read: the repository set with <see cref="DefaultFilter"/> applied.
        /// </summary>
        protected virtual IQueryable<TEntity> BaseQuery()
        {
            return Repository.Get().Where(DefaultFilter());
        }

        private IQueryable<TEntity> ById(Guid id)
        {
            return BaseQuery().Where(e => e.Id == id);
        }

        /// <summary>
        /// Builds a runtime type with only the requested fields that exist in the view.
        /// Fields outside the view are ignored, so hidden entity properties cannot be requested.
        /// </summary>
        private static Type FieldsType(List<string> fields)
        {
            var propertyMap = typeof(TView)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => fields.Contains(p.Name, StringComparer.OrdinalIgnoreCase))
                .ToDictionary(p => p.Name, p => p.PropertyType);

            if (propertyMap.Count == 0)
            {
                throw new InvalidQueryException("None of the requested fields exist.");
            }

            return CustomTypeBuilder.CreateType(propertyMap);
        }
    }
}
