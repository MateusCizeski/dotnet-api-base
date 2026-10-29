using ApiBase.Domain.Interfaces;
using ApiBase.Sample.Data;
using ApiBase.Sample.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ApiBase.Tests.Integration
{
    public class SoftDeleteTests
    {
        [Fact]
        public async Task Remove_SoftDeletableEntity_KeepsRowAndHidesItEverywhere()
        {
            using var services = new SqliteServices();
            Guid appleId;

            using (var scope = services.Provider.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<IRepositoryBase<Product>>();
                var apple = await repository.FirstOrDefaultAsync(p => p.Name == "Apple");
                appleId = apple!.Id;

                repository.Remove(apple);
                await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().CommitAsync();
            }

            using (var scope = services.Provider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
                var repository = scope.ServiceProvider.GetRequiredService<IRepositoryBase<Product>>();

                // Repository and direct DbSet queries hide it
                Assert.Null(await repository.GetByIdAsync(appleId));
                Assert.False(await db.Products.AnyAsync(p => p.Id == appleId));

                // Include also hides it (the old per-method filter did not)
                var fruit = await db.Categories.Include(c => c.Products).SingleAsync(c => c.Name == "Fruit");
                Assert.DoesNotContain(fruit.Products, p => p.Id == appleId);

                // The row is still there
                var deleted = await db.Products.IgnoreQueryFilters().SingleAsync(p => p.Id == appleId);
                Assert.True(deleted.IsDeleted);
                Assert.NotNull(deleted.DeletedAt);
            }
        }
    }
}
