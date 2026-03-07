using ApiBase.Domain.Entities;
using ApiBase.Domain.Interfaces;
using ApiBase.Infra.UnitOfWork;

namespace ApiBase.Application.Base
{
    public class ApplicationBase<TEntity, TRepository> where TEntity : EntityGuid, new() where TRepository : IRepositoryBase<TEntity>
    {
        protected IUnitOfWork UnitOfWork { get; set; }
        protected TRepository Repository { get; set; }

        protected ApplicationBase(IUnitOfWork unitOfWork, TRepository repository)
        {
            UnitOfWork = unitOfWork;
            Repository = repository;
        }

        protected void Commit()
        {
            UnitOfWork.Commit();
        }

        protected void RollbackChanges()
        {
            UnitOfWork.RollbackChanges();
        }
    }
}
