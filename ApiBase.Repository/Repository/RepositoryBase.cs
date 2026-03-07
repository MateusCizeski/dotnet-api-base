using ApiBase.Domain.Entities;
using ApiBase.Domain.Interfaces;
using ApiBase.Domain.Query;
using ApiBase.Infra.Query;
using ApiBase.Repository.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace ApiBase.Repository.Repositories
{
    /// <summary>
    /// Generic base repository providing full CRUD, async operations and automatic soft delete filtering.
    /// Entities implementing <see cref="ISoftDelete"/> are never physically removed —
    /// <see cref="Remove(T)"/> sets IsDeleted/DeletedAt instead, and all queries automatically
    /// exclude soft-deleted records.
    /// </summary>
    public abstract class RepositoryBase<T> : IRepositoryBase<T>, IDisposable where T : EntityGuid
    {
        protected readonly DbContext Db;
        protected readonly DbSet<T> DbSet;
        protected readonly ILogger Logger;

        protected RepositoryBase(Context context, ILogger logger)
        {
            Db = context;
            DbSet = Db.Set<T>();
            Logger = logger;
        }

        public void Dispose()
        {
            // DbContext lifecycle is managed by the DI container (Scoped via AddDbContext).
            GC.SuppressFinalize(this);
        }

        // -------------------------
        // Write (sync)
        // -------------------------

        /// <inheritdoc/>
        public virtual void Insert(T entity)
        {
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }

            DbSet.Add(entity);
            Logger.LogDebug("Insert queued for {Entity} with Id {Id}.", typeof(T).Name, entity.Id);
        }

        /// <inheritdoc/>
        public virtual void Insert(List<T> entities)
        {
            foreach (var entity in entities)
            {
                if (entity.Id == Guid.Empty)
                {
                    entity.Id = Guid.NewGuid();
                }
            }

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
            }
            else
            {
                DbSet.Remove(entity);
                Logger.LogDebug("Hard delete queued for {Entity} with Id {Id}.", typeof(T).Name, entity.Id);
            }
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
        public virtual async Task InsertAsync(T entity)
        {
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }

            await DbSet.AddAsync(entity);
            Logger.LogDebug("InsertAsync queued for {Entity} with Id {Id}.", typeof(T).Name, entity.Id);
        }

        /// <inheritdoc/>
        public virtual async Task InsertAsync(List<T> entities)
        {
            foreach (var entity in entities)
            {
                if (entity.Id == Guid.Empty)
                {
                    entity.Id = Guid.NewGuid();
                }
            }

            await DbSet.AddRangeAsync(entities);
            Logger.LogDebug("InsertAsync queued for {Count} {Entity} record(s).", entities.Count, typeof(T).Name);
        }

        /// <inheritdoc/>
        public virtual async Task RemoveAsync(Guid id)
        {
            var entity = await DbSet.FindAsync(id);
            
            if (entity != null)
            {
                Remove(entity);
            }
        }

        // -------------------------
        // Read (sync)
        // -------------------------

        /// <inheritdoc/>
        public T GetById(Guid id, params string[] includes)
        {
            return Get(includes).FirstOrDefault(x => x.Id == id);
        }

        /// <inheritdoc/>
        public IQueryable<T> Get(params string[] includes)
        {
            IQueryable<T> query = includes.Length == 0 ? DbSet : includes.Aggregate(DbSet.AsQueryable(), (q, path) => q.Include(path));

            return ApplySoftDeleteFilter(query);
        }

        /// <inheritdoc/>
        public virtual IQueryable<T> Get(QueryParams queryParams)
        {
            var filters = queryParams.GetFilters();
            var includes = queryParams.GetIncludes();
            var order = queryParams.GetSort() ?? DefaultOrder();
            return Get(filters, order, includes.ToArray());
        }

        /// <inheritdoc/>
        public IQueryable<T> Get(List<FilterModel> filters, List<SortModel> order, params string[] includes)
        {
            return Get(new List<FilterGroup> { new FilterGroup { Filters = filters } }, order, includes);
        }

        /// <inheritdoc/>
        public IQueryable<T> Get(List<FilterGroup> filters, List<SortModel> order, params string[] includes)
        {
            IQueryable<T> query = DbSet.AsQueryable();

            if (includes != null)
            {
                foreach (var path in includes)
                {
                    query = query.Include(path);
                }
            }

            query = ApplySoftDeleteFilter(query);
            return new QueryBuilder<T>().Build(query, filters, order);
        }

        /// <inheritdoc/>
        public IQueryable<T> Where(Expression<Func<T, bool>> expression)
        {
            return ApplySoftDeleteFilter(DbSet).Where(expression);
        }

        /// <inheritdoc/>
        public T FirstOrDefault()
        {
            return ApplySoftDeleteFilter(DbSet).FirstOrDefault();
        }

        // -------------------------
        // Read (async)
        // -------------------------

        /// <inheritdoc/>
        public async Task<T> GetByIdAsync(Guid id, params string[] includes)
        {
            return await Get(includes).FirstOrDefaultAsync(x => x.Id == id);
        }

        /// <inheritdoc/>
        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> expression)
        {
            return await ApplySoftDeleteFilter(DbSet).FirstOrDefaultAsync(expression);
        }

        /// <inheritdoc/>
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> expression)
        {
            return await ApplySoftDeleteFilter(DbSet).AnyAsync(expression);
        }

        // -------------------------
        // Helpers
        // -------------------------

        /// <summary>
        /// Automatically filters out soft-deleted records for entities implementing <see cref="ISoftDelete"/>.
        /// For entities that do not implement ISoftDelete, returns the query unchanged.
        /// </summary>
        private IQueryable<T> ApplySoftDeleteFilter(IQueryable<T> query)
        {
            if (typeof(ISoftDelete).IsAssignableFrom(typeof(T)))
            {
                return query.Where(e => !((ISoftDelete)e).IsDeleted);
            }

            return query;
        }

        private List<SortModel> DefaultOrder()
        {
            return new List<SortModel>
            {
                new SortModel { Property = "Id", Direction = "asc" }
            };
        }
    }
}
