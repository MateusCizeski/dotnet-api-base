using ApiBase.Domain.Entities;

namespace ApiBase.Domain.Interfaces
{
    /// <summary>
    /// Coordinates the work of multiple repositories by providing a single transaction boundary.
    /// Call <see cref="CommitAsync"/> (preferred) or <see cref="Commit"/> to persist all pending changes.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        // -------------------------
        // Transaction (sync)
        // -------------------------

        /// <summary>
        /// Validates all tracked entities and persists all pending changes to the database.
        /// Rolls back automatically on failure.
        /// </summary>
        void Commit();

        /// <summary>
        /// Discards all pending changes by resetting tracked entity states.
        /// </summary>
        void RollbackChanges();

        // -------------------------
        // Transaction (async)
        // -------------------------

        /// <summary>
        /// Asynchronously validates all tracked entities and persists all pending changes.
        /// Rolls back automatically on failure.
        /// </summary>
        Task CommitAsync();

        /// <summary>
        /// Asynchronously discards all pending changes.
        /// </summary>
        Task RollbackChangesAsync();

        // -------------------------
        // Custom fields
        // -------------------------

        /// <summary>
        /// Merges any custom/dynamic fields into a list of result objects.
        /// </summary>
        IList<object> BuildCustomFieldsList<T>(List<object> pagedResults) where T : EntityGuid, new();

        /// <summary>
        /// Merges any custom/dynamic fields into a single result object.
        /// </summary>
        object BuildCustomFieldsList<T>(object result) where T : EntityGuid, new();
    }
}
