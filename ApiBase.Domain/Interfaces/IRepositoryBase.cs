using ApiBase.Domain.Entities;
using ApiBase.Domain.Query;
using System.Linq.Expressions;

namespace ApiBase.Domain.Interfaces
{
    /// <summary>
    /// Generic repository contract providing standard CRUD and query operations.
    /// All write operations require an explicit <see cref="IUnitOfWork.CommitAsync"/> or
    /// <see cref="IUnitOfWork.Commit"/> call to persist changes.
    /// Soft-deleted records (<see cref="ISoftDelete"/>) never appear in the results.
    /// </summary>
    /// <typeparam name="T">Entity type derived from <see cref="EntityGuid"/>.</typeparam>
    public interface IRepositoryBase<T> where T : EntityGuid
    {
        // -------------------------
        // Write operations (sync)
        // -------------------------

        /// <summary>Adds a new entity.</summary>
        void Insert(T entity);

        /// <summary>Adds a list of entities.</summary>
        void Insert(List<T> entities);

        /// <summary>
        /// Updates an existing entity by attaching it to the context and marking it as modified.
        /// </summary>
        void Update(T entity);

        /// <summary>Removes the given entity (soft delete when it implements <see cref="ISoftDelete"/>).</summary>
        void Remove(T entity);

        /// <summary>Removes the entity with the given Id. No-op if not found.</summary>
        void Remove(Guid id);

        /// <summary>Removes a list of entities.</summary>
        void Remove(List<T> entities);

        // -------------------------
        // Write operations (async)
        // -------------------------

        /// <summary>Asynchronously adds a new entity.</summary>
        Task InsertAsync(T entity, CancellationToken cancellationToken = default);

        /// <summary>Asynchronously adds a list of entities.</summary>
        Task InsertAsync(List<T> entities, CancellationToken cancellationToken = default);

        /// <summary>Asynchronously removes the entity with the given Id. No-op if not found.</summary>
        Task RemoveAsync(Guid id, CancellationToken cancellationToken = default);

        // -------------------------
        // Read operations (sync)
        // -------------------------

        /// <summary>Returns the entity with the given Id, optionally including navigation properties.</summary>
        T? GetById(Guid id, params string[] includes);

        /// <summary>Returns all entities as a queryable, optionally including navigation properties.</summary>
        IQueryable<T> Get(params string[] includes);

        /// <summary>Returns entities matching the given flat filters, sort and includes.</summary>
        IQueryable<T> Get(List<FilterModel> filters, List<SortModel> order, params string[] includes);

        /// <summary>Returns entities matching the given grouped filters, sort and includes.</summary>
        IQueryable<T> Get(List<FilterGroup> filters, List<SortModel> order, params string[] includes);

        /// <summary>Returns the first entity in the set, or null if empty.</summary>
        T? FirstOrDefault();

        /// <summary>Returns entities matching the given predicate.</summary>
        IQueryable<T> Where(Expression<Func<T, bool>> expression);

        // -------------------------
        // Read operations (async)
        // -------------------------

        /// <summary>Asynchronously returns the entity with the given Id, or null.</summary>
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>Asynchronously returns the entity with the given Id including navigation properties, or null.</summary>
        Task<T?> GetByIdAsync(Guid id, string[] includes, CancellationToken cancellationToken = default);

        /// <summary>Asynchronously returns the first entity matching the given predicate, or null.</summary>
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default);

        /// <summary>Asynchronously returns whether any entity matches the given predicate.</summary>
        Task<bool> AnyAsync(Expression<Func<T, bool>> expression, CancellationToken cancellationToken = default);
    }
}
