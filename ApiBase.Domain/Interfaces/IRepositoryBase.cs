using ApiBase.Domain.Entities;
using ApiBase.Domain.Query;
using System.Linq.Expressions;

namespace ApiBase.Domain.Interfaces
{
    /// <summary>
    /// Generic repository contract providing standard CRUD and query operations.
    /// All write operations require an explicit <see cref="IUnitOfWork.CommitAsync"/> or
    /// <see cref="IUnitOfWork.Commit"/> call to persist changes.
    /// </summary>
    /// <typeparam name="T">Entity type derived from <see cref="EntityGuid"/>.</typeparam>
    public interface IRepositoryBase<T> : IDisposable where T : EntityGuid
    {
        // -------------------------
        // Write operations (sync)
        // -------------------------

        /// <summary>Adds a new entity. Generates a new Id if empty.</summary>
        void Insert(T entity);

        /// <summary>Adds a list of entities. Generates new Ids for any with empty Id.</summary>
        void Insert(List<T> entities);

        /// <summary>
        /// Updates an existing entity by attaching it to the context and marking it as modified.
        /// </summary>
        void Update(T entity);

        /// <summary>Removes the given entity.</summary>
        void Remove(T entity);

        /// <summary>Removes the entity with the given Id. No-op if not found.</summary>
        void Remove(Guid id);

        /// <summary>Removes a list of entities.</summary>
        void Remove(List<T> entities);

        // -------------------------
        // Write operations (async)
        // -------------------------

        /// <summary>Asynchronously adds a new entity. Generates a new Id if empty.</summary>
        Task InsertAsync(T entity);

        /// <summary>Asynchronously adds a list of entities.</summary>
        Task InsertAsync(List<T> entities);

        /// <summary>Asynchronously removes the entity with the given Id. No-op if not found.</summary>
        Task RemoveAsync(Guid id);

        // -------------------------
        // Read operations (sync)
        // -------------------------

        /// <summary>Returns the entity with the given Id, optionally including navigation properties.</summary>
        T GetById(Guid id, params string[] includes);

        /// <summary>Returns all entities as a queryable, optionally including navigation properties.</summary>
        IQueryable<T> Get(params string[] includes);

        /// <summary>Returns entities matching the given flat filters, sort and includes.</summary>
        IQueryable<T> Get(List<FilterModel> filters, List<SortModel> order, params string[] includes);

        /// <summary>Returns entities matching the given grouped filters, sort and includes.</summary>
        IQueryable<T> Get(List<FilterGroup> filters, List<SortModel> order, params string[] includes);

        /// <summary>Returns the first entity in the set, or null if empty.</summary>
        T FirstOrDefault();

        /// <summary>Returns entities matching the given predicate.</summary>
        IQueryable<T> Where(Expression<Func<T, bool>> expression);

        // -------------------------
        // Read operations (async)
        // -------------------------

        /// <summary>Asynchronously returns the entity with the given Id.</summary>
        Task<T> GetByIdAsync(Guid id, params string[] includes);

        /// <summary>Asynchronously returns the first entity matching the given predicate, or null.</summary>
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> expression);

        /// <summary>Asynchronously returns whether any entity matches the given predicate.</summary>
        Task<bool> AnyAsync(Expression<Func<T, bool>> expression);
    }
}
