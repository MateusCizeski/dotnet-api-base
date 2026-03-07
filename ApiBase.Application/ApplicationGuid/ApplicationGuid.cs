using ApiBase.Application.Base;
using ApiBase.Domain.Entities;
using ApiBase.Domain.Interfaces;
using ApiBase.Domain.Query;
using ApiBase.Domain.View;
using ApiBase.Infra.Extensions;
using ApiBase.Infra.Helpers;
using ApiBase.Infra.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Reflection;

namespace ApiBase.Application.ApplicationGuid
{
    /// <summary>
    /// Base application service for entities with a GUID primary key.
    /// Provides standard read operations (sync and async) with pagination,
    /// filtering, field projection, dynamic sorting and structured logging.
    /// Override <see cref="DefaultFilter"/> to apply cross-cutting query filters
    /// such as tenant isolation or ownership checks.
    /// </summary>
    public abstract class ApplicationGuid<TEntity, TRepository, TView>
        : ApplicationBase<TEntity, TRepository>, IApplicationGuid<TView>
        where TEntity : EntityGuid, new()
        where TRepository : IRepositoryBase<TEntity>
        where TView : IdGuidView, new()
    {
        public ApplicationGuid(IUnitOfWork unitOfWork, TRepository repository, ILogger logger) : base(unitOfWork, repository, logger) { }

        // -------------------------
        // Read (sync)
        // -------------------------

        /// <inheritdoc/>
        public virtual GetView Get(QueryParams queryParams)
        {
            Logger.LogDebug("Get list called for {Entity}.", typeof(TEntity).Name);

            IQueryable<TEntity> query = Repository.Get().Where(DefaultFilter());
            GetView result = new GuidQueryHelper().Page<TEntity, TView>(query, queryParams);
            result.Content = UnitOfWork.BuildCustomFieldsList<TEntity>(result.Content.Cast<object>().ToList());

            Logger.LogDebug("Get list returned {Total} record(s) for {Entity}.", result.Total, typeof(TEntity).Name);

            return result;
        }

        /// <inheritdoc/>
        public virtual object Get(Guid id)
        {
            Logger.LogDebug("Get by Id called for {Entity} Id={Id}.", typeof(TEntity).Name, id);

            IQueryable<TEntity> query = Repository.Where(e => e.Id == id).Where(DefaultFilter());
            var view = query.Project().To<TView>().FirstOrDefault();

            if (view == null)
            {
                Logger.LogDebug("{Entity} Id={Id} not found.", typeof(TEntity).Name, id);
                return new { };
            }

            var withCustomFields = UnitOfWork.BuildCustomFieldsList<TEntity>(new List<object> { view });
            return withCustomFields.FirstOrDefault() ?? new { };
        }

        /// <inheritdoc/>
        public object Get(Guid id, List<string> fields)
        {
            Logger.LogDebug("Get by Id with fields called for {Entity} Id={Id}. Fields: {Fields}.",
                typeof(TEntity).Name, id, string.Join(", ", fields ?? []));

            IQueryable<TEntity> query = Repository.Where(e => e.Id == id).Where(DefaultFilter());

            if (fields == null || fields.Count == 0)
            {
                var view = query.Project().To<TView>().FirstOrDefault();
                if (view == null)
                {
                    Logger.LogDebug("{Entity} Id={Id} not found.", typeof(TEntity).Name, id);
                    return new { };
                }
                return view;
            }

            var propertyMap = typeof(TView)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => fields.Contains(p.Name))
                .ToDictionary(p => p.Name, p => p.PropertyType);

            var dynamicType = CustomTypeBuilder.CreateType(propertyMap);
            var result = query.Project().To(dynamicType).FirstOrDefault();

            if (result == null)
            {
                Logger.LogDebug("{Entity} Id={Id} not found.", typeof(TEntity).Name, id);
                return new { };
            }

            return result;
        }

        // -------------------------
        // Read (async)
        // -------------------------

        /// <inheritdoc/>
        public virtual async Task<GetView> GetAsync(QueryParams queryParams)
        {
            Logger.LogDebug("GetAsync list called for {Entity}.", typeof(TEntity).Name);

            IQueryable<TEntity> query = Repository.Get().Where(DefaultFilter());

            GetView result = await Task.Run(() => new GuidQueryHelper().Page<TEntity, TView>(query, queryParams));

            result.Content = UnitOfWork.BuildCustomFieldsList<TEntity>(result.Content.Cast<object>().ToList());

            Logger.LogDebug("GetAsync list returned {Total} record(s) for {Entity}.", result.Total, typeof(TEntity).Name);

            return result;
        }

        /// <inheritdoc/>
        public virtual async Task<object> GetAsync(Guid id)
        {
            Logger.LogDebug("GetAsync by Id called for {Entity} Id={Id}.", typeof(TEntity).Name, id);

            IQueryable<TEntity> query = Repository.Where(e => e.Id == id).Where(DefaultFilter());
            var view = await query.Project().To<TView>().FirstOrDefaultAsync();

            if (view == null)
            {
                Logger.LogDebug("{Entity} Id={Id} not found.", typeof(TEntity).Name, id);
                return new { };
            }

            var withCustomFields = UnitOfWork.BuildCustomFieldsList<TEntity>(new List<object> { view });
            return withCustomFields.FirstOrDefault() ?? new { };
        }

        /// <inheritdoc/>
        public async Task<object> GetAsync(Guid id, List<string> fields)
        {
            Logger.LogDebug("GetAsync by Id with fields called for {Entity} Id={Id}. Fields: {Fields}.",
                typeof(TEntity).Name, id, string.Join(", ", fields ?? []));

            IQueryable<TEntity> query = Repository.Where(e => e.Id == id).Where(DefaultFilter());

            if (fields == null || fields.Count == 0)
            {
                var view = await query.Project().To<TView>().FirstOrDefaultAsync();

                if (view == null)
                {
                    Logger.LogDebug("{Entity} Id={Id} not found.", typeof(TEntity).Name, id);
                    return new { };
                }

                return view;
            }

            var propertyMap = typeof(TView)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => fields.Contains(p.Name))
                .ToDictionary(p => p.Name, p => p.PropertyType);

            var dynamicType = CustomTypeBuilder.CreateType(propertyMap);
            var result = await query.Project().To(dynamicType).FirstOrDefaultAsync();

            if (result == null)
            {
                Logger.LogDebug("{Entity} Id={Id} not found.", typeof(TEntity).Name, id);
                return new { };
            }

            return result;
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
    }
}