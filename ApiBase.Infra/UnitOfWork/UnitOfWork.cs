using ApiBase.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace ApiBase.Infra.UnitOfWork
{
    /// <summary>
    /// Coordinates persistence of changes across repositories within a single DbContext.
    /// Provides both sync and async commit/rollback with automatic entity validation and structured logging.
    /// The DbContext is the same scoped instance the repositories use, and it is owned by the DI
    /// container — this class never disposes it.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext _dbContext;
        private readonly ILogger<UnitOfWork> _logger;

        public UnitOfWork(DbContext dbContext, ILogger<UnitOfWork> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // -------------------------
        // Sync
        // -------------------------

        /// <inheritdoc/>
        public void Commit()
        {
            try
            {
                ValidateEntities();
                var changes = _dbContext.SaveChanges();
                _logger.LogDebug("Commit successful. {Changes} record(s) affected.", changes);
            }
            catch (Exception ex) when (ex is DbUpdateException or ValidationException)
            {
                _logger.LogError(ex, "Commit failed. Tracked changes will be reverted.");
                RollbackChanges();
                throw;
            }
        }

        /// <inheritdoc/>
        public void RollbackChanges()
        {
            var changedEntries = _dbContext.ChangeTracker.Entries().Where(e => e.State != EntityState.Unchanged).ToList();

            foreach (var entry in changedEntries)
            {
                switch (entry.State)
                {
                    case EntityState.Modified:
                        entry.CurrentValues.SetValues(entry.OriginalValues);
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Unchanged;
                        break;
                }
            }

            _logger.LogDebug("Rollback completed. {Count} tracked change(s) reverted.", changedEntries.Count);
        }

        // -------------------------
        // Async
        // -------------------------

        /// <inheritdoc/>
        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                ValidateEntities();
                var changes = await _dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogDebug("CommitAsync successful. {Changes} record(s) affected.", changes);
            }
            catch (Exception ex) when (ex is DbUpdateException or ValidationException)
            {
                _logger.LogError(ex, "CommitAsync failed. Tracked changes will be reverted.");
                await RollbackChangesAsync();
                throw;
            }
        }

        /// <inheritdoc/>
        public Task RollbackChangesAsync()
        {
            // EF Core's ChangeTracker rollback is synchronous — wrapped in Task for interface consistency
            RollbackChanges();
            return Task.CompletedTask;
        }

        // -------------------------
        // Validation
        // -------------------------

        private void ValidateEntities()
        {
            var entitiesToValidate = _dbContext.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .Select(e => e.Entity);

            foreach (var entity in entitiesToValidate)
            {
                var validationContext = new ValidationContext(entity);
                Validator.ValidateObject(entity, validationContext, validateAllProperties: true);
            }
        }
    }
}
