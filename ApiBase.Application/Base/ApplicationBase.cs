using ApiBase.Domain.Entities;
using ApiBase.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ApiBase.Application.Base
{
    /// <summary>
    /// Base application service providing access to the unit of work, repository and logger.
    /// All application services should inherit from this class.
    /// There is no <c>new()</c> constraint on the entity: rich domain entities with
    /// protected constructors are supported.
    /// </summary>
    public class ApplicationBase<TEntity, TRepository> where TEntity : EntityGuid where TRepository : IRepositoryBase<TEntity>
    {
        protected IUnitOfWork UnitOfWork { get; }
        protected TRepository Repository { get; }
        protected ILogger Logger { get; }

        protected ApplicationBase(IUnitOfWork unitOfWork, TRepository repository, ILogger logger)
        {
            UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>Persists all pending changes synchronously.</summary>
        protected void Commit()
        {
            UnitOfWork.Commit();
        }

        /// <summary>Persists all pending changes asynchronously.</summary>
        protected Task CommitAsync(CancellationToken cancellationToken = default)
        {
            return UnitOfWork.CommitAsync(cancellationToken);
        }

        /// <summary>Discards all pending changes synchronously.</summary>
        protected void RollbackChanges()
        {
            UnitOfWork.RollbackChanges();
        }

        /// <summary>Discards all pending changes asynchronously.</summary>
        protected Task RollbackChangesAsync()
        {
            return UnitOfWork.RollbackChangesAsync();
        }
    }
}
