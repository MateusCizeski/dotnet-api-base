namespace ApiBase.Domain.Interfaces
{
    /// <summary>
    /// Coordinates the work of multiple repositories by providing a single transaction boundary.
    /// Call <see cref="CommitAsync"/> (preferred) or <see cref="Commit"/> once per use case
    /// to persist all pending changes.
    /// The underlying DbContext is owned by the DI container — the unit of work never disposes it.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Validates all tracked entities and persists all pending changes to the database.
        /// Reverts the tracked changes on failure.
        /// </summary>
        void Commit();

        /// <summary>
        /// Discards all pending changes by resetting tracked entity states.
        /// </summary>
        void RollbackChanges();

        /// <summary>
        /// Asynchronously validates all tracked entities and persists all pending changes.
        /// Reverts the tracked changes on failure.
        /// </summary>
        Task CommitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously discards all pending changes.
        /// </summary>
        Task RollbackChangesAsync();
    }
}
