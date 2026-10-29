using ApiBase.Domain.Interfaces;
using ApiBase.Sample.Data;
using ApiBase.Sample.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ApiBase.Tests.Integration
{
    public class ServiceRegistrationTests
    {
        [Fact]
        public void AddApiBaseCore_ResolvesRepositoryAndUnitOfWork()
        {
            using var services = new SqliteServices();
            using var scope = services.Provider.CreateScope();

            Assert.NotNull(scope.ServiceProvider.GetRequiredService<IUnitOfWork>());
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<IRepositoryBase<Product>>());
        }

        [Fact]
        public void AddApiBaseCore_SharesTheSameDbContextInTheScope()
        {
            using var services = new SqliteServices();
            using var scope = services.Provider.CreateScope();

            var typed = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
            var baseType = scope.ServiceProvider.GetRequiredService<DbContext>();

            Assert.Same(typed, baseType);
        }

        [Fact]
        public async Task InsertThroughRepository_CommitThroughUnitOfWork_Persists()
        {
            using var services = new SqliteServices();
            Guid id;

            using (var scope = services.Provider.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<IRepositoryBase<Category>>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var category = new Category("Dairy");
                id = category.Id;

                await repository.InsertAsync(category);
                await unitOfWork.CommitAsync();
            }

            using (var scope = services.Provider.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<IRepositoryBase<Category>>();
                var saved = await repository.GetByIdAsync(id);

                Assert.NotNull(saved);
                Assert.Equal("Dairy", saved.Name);
            }
        }
    }
}
