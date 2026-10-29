using ApiBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ApiBase.Repository.Repositories
{
    /// <summary>
    /// Concrete generic repository registered by <c>AddApiBaseCore</c> as the default
    /// implementation of <c>IRepositoryBase&lt;T&gt;</c>. Lets read-only services use
    /// <c>IRepositoryBase&lt;Product&gt;</c> without writing a repository class.
    /// Create your own class inheriting <see cref="RepositoryBase{T}"/> when you need custom queries.
    /// </summary>
    public class EfRepository<T> : RepositoryBase<T> where T : EntityGuid
    {
        public EfRepository(DbContext context, ILogger<EfRepository<T>> logger) : base(context, logger) { }
    }
}
