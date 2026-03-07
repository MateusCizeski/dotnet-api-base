using ApiBase.Domain.Entities;
using ApiBase.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ApiBase.Infra.UnitOfWork
{
    /// <summary>
    /// Coordinates persistence of changes across repositories within a single DbContext.
    /// Provides both sync and async commit/rollback with automatic entity validation and structured logging.
    /// </summary>
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly DbContext _dbContext;
        private readonly ILogger<UnitOfWork> _logger;

        public UnitOfWork(DbContext dbContext, ILogger<UnitOfWork> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Dispose()
        {
            _dbContext.Dispose();
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
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Commit failed due to a database update error.");
                RollbackChanges();
                throw new DbUpdateException("Database update failed: " + ex.GetBaseException().Message, ex);
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, "Commit failed due to a validation error.");
                RollbackChanges();
                throw new ValidationException("Validation failed: " + ex.Message, ex.ValidationAttribute, ex.Value);
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
        public async Task CommitAsync()
        {
            try
            {
                ValidateEntities();
                var changes = await _dbContext.SaveChangesAsync();
                _logger.LogDebug("CommitAsync successful. {Changes} record(s) affected.", changes);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "CommitAsync failed due to a database update error.");
                await RollbackChangesAsync();
                throw new DbUpdateException("Database update failed: " + ex.GetBaseException().Message, ex);
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, "CommitAsync failed due to a validation error.");
                await RollbackChangesAsync();
                throw new ValidationException("Validation failed: " + ex.Message, ex.ValidationAttribute, ex.Value);
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
        // Custom fields
        // -------------------------

        /// <inheritdoc/>
        public IList<object> BuildCustomFieldsList<T>(List<object> pagedResults) where T : EntityGuid, new()
        {
            return pagedResults.Select(obj => MergeCustomFields<T>(obj)).ToList();
        }

        /// <inheritdoc/>
        public object BuildCustomFieldsList<T>(object result) where T : EntityGuid, new()
        {
            return MergeCustomFields<T>(result);
        }

        private object MergeCustomFields<T>(object source) where T : EntityGuid, new()
        {
            if (source is IDictionary<string, object> dict)
            {
                return new Dictionary<string, object>(dict);
            }

            var target = new Dictionary<string, object>();
            var props = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                target[prop.Name] = prop.GetValue(source);
            }

            return target;
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
