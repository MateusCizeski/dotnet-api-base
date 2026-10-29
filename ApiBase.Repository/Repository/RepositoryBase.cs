using ApiBase.Domain.Entities;
using ApiBase.Domain.Interfaces;
using ApiBase.Domain.Query;
using ApiBase.Infra.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace ApiBase.Repository.Repositories
{
    /// <summary>
    /// Generic base repository providing CRUD and async operations over a DbContext.
    /// Entities implementing <see cref="ISoftDelete"/> are never physically removed —
    /// <see cref="Remove(T)"/> sets IsDeleted/DeletedAt instead. Reads hide soft-deleted
    /// records through the global query filter (see <c>ApplySoftDeleteQueryFilters</c>).
    /// </summary>
    public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : EntityGuid
    {
        protected readonly DbContext Db;
        protected readonly DbSet<T> DbSet;
        protected readonly ILogger Logger;

        /// <param name="context">
        /// The scoped DbContext. <c>AddApiBaseCore&lt;TContext&gt;</c> registers your context
        /// also as <see cref="DbContext"/>, so the repository and the unit of work share the same instance.
        /// </param>
        /// <param name="logger">Logger used for debug traces of queued writes.</param>
        protected RepositoryBase(DbContext context, ILogger logger)
        {
            Db = context ?? throw new ArgumentNullException(nameof(context));
            DbSet = Db.Set<T>();
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // -------------------------
        // Write (sync)
        // -------------------------

        /// <inheritdoc/>
        public virtual void Insert(T entity)
        {
            DbSet.Add(entity);
            Logger.LogDebug("Insert queued for {Entity} with Id {Id}.", typeof(T).Name, entity.Id);
        }

        /// <inheritdoc/>
        public virtual void Insert(List<T> entities)
        {
            DbSet.AddRange(entities);
            Logger.LogDebug("Insert queued for {Count} {Entity} record(s).", entities.Count, typeof(T).Name);
        }

        /// <inheritdoc/>
        public virtual void Update(T entity)
        {
            DbSet.Update(entity);
            Logger.LogDebug("Update queued for {Entity} with Id {Id}.", typeof(T).Name, entity.Id);
        }

        /// <inheritdoc/>
        public virtual void Remove(T entity)
        {
            // If entity supports soft delete, mark as deleted instead of removing physically
            if (entity is ISoftDelete softDeletable)
            {
                softDeletable.IsDeleted = true;
                softDeletable.DeletedAt = DateTime.UtcNow;
                DbSet.Update(entity);
                Logger.LogDebug("Soft delete queued for {Entity} with Id {Id}.", typeof(T).Name, entity.Id);
                return;
            }

            DbSet.Remove(entity);
            Logger.LogDebug("Hard delete queued for {Entity} with Id {Id}.", typeof(T).Name, entity.Id);
        }

        /// <inheritdoc/>
        public virtual void Remove(Guid id)
        {
            var entity = DbSet.Find(id);

            if (entity != null)
            {
                Remove(entity);
            }
        }

        /// <inheritdoc/>
        public virtual void Remove(List<T> entities)
        {
            foreach (var entity in entities)
            {
                Remove(entity);
            }
        }

        // -------------------------
        // Write (async)
        // -------------------------

        /// <inheritdoc/>
        public virtual async Task InsertAsync(T entity, CancellationToken cancellationToken = default)
        {
            await DbSet.AddAsync(entity, cancellationToken);
            Logger.LogDebug("InsertAsync queued for {Entity} with Id {Id}.", typeof(T).Name, entity.Id);
        }

        /// <inheritdoc/>
        public virtual async Task InsertAsync(List<T> entities, CancellationToken cancellationToken = default)
        {
            await DbSet.AddRangeAsync(entities, cancellationToken);
            Logger.LogDebug("InsertAsync queued for {Count} {Entity} record(s).", entities.Count, typeof(T).Name);
        }

        /// <inheritdoc/>
        public virtual async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await DbSet.FindAsync([id], cancellationToken);

            if (entity != null)
            {
                Remove(entity);
            }
        }

        // -------------------------
        // Read (sync)
        // -------------------------

        /// <inheritdoc/>
        public virtual T? GetById(Guid id, params string[] includes)
        {
            return Get(includes).FirstOrDefault(x => x.Id == id);
        }

        /// <inheritdoc/>
        public virtual IQueryable<T> Get(params string[] includes)
        {
            IQueryable<T> query = DbSet;

            foreach (var path in includes ?? [])
            {
                query = query.Include(path);
            }

            return query;
        }

        /// <inheritdoc/>
        public IQueryable<T> Get(List<FilterModel> filters, List<SortModel> order, params string[] includes)
        {
            return Get(new List<FilterGroup> { new FilterGroup { Filters = filters } }, order, includes);
        }

        /// <inheritdoc/>
        public IQueryable<T> Get(List<FilterGroup> filters, List<SortModel> order, params string[] includes)
        {
            return new QueryBuilder<T>().Build(Get(includes), filters, order ?? DefaultOrder());
        }

        /// <inheritdoc/>
        public IQueryable<T> Where(Expression<Func<T, bool>> expression)
        {
            return DbSet.Where(expression);
        }

        /// <inheritdoc/>
        public T? FirstOrDefault()
        {
            return DbSet.FirstOrDefault();
        }

        // -------------------------
        // Read (async)
        // -------------------------

        /// <inheritdoc/>
        public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return GetByIdAsync(id, [], cancellationToken);
        }

        /// <inheritdoc/>
        public virtual Task<T?> GetByIdAsync(Guid id, string[] includes, CancellationToken cancellationToken = default)
        {
            return Get(includes).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default)
        {
            return DbSet.FirstOrDefaultAsync(expression, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<bool> AnyAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default)
        {
            return DbSet.AnyAsync(expression, cancellationToken);
        }

        // -------------------------
        // Helpers
        // -------------------------

        private static List<SortModel> DefaultOrder()
        {
            return new List<SortModel>
            {
                new SortModel { Property = "Id", Direction = "asc" }
            };
        }
    }
}
