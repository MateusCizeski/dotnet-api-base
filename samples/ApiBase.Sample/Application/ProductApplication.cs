using ApiBase.Application.ApplicationGuid;
using ApiBase.Domain.Exceptions;
using ApiBase.Domain.Interfaces;
using ApiBase.Infra.Options;
using ApiBase.Sample.Domain;
using Microsoft.Extensions.Options;

namespace ApiBase.Sample.Application
{
    public interface IProductApplication : IApplicationGuid<ProductView>
    {
        Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Reads come from ApplicationGuid (filter, sort, paging, projection).
    /// The generic IRepositoryBase&lt;Product&gt; is enough: no repository class is needed.
    /// </summary>
    public class ProductApplication : ApplicationGuid<Product, IRepositoryBase<Product>, ProductView>, IProductApplication
    {
        public ProductApplication(IUnitOfWork unitOfWork, IRepositoryBase<Product> repository, ILogger<ProductApplication> logger, IOptions<ApiBaseOptions> options)
            : base(unitOfWork, repository, logger, options) { }

        /// <summary>A command: change through the repository, then one commit.</summary>
        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var product = await Repository.GetByIdAsync(id, cancellationToken) ?? throw NotFoundException.For<Product>(id);

            Repository.Remove(product); // soft delete: Product implements ISoftDelete
            await CommitAsync(cancellationToken);
        }
    }
}
