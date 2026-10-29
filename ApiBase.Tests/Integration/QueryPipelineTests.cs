using ApiBase.Domain.Exceptions;
using ApiBase.Domain.Interfaces;
using ApiBase.Infra.Query;
using ApiBase.Sample.Application;
using ApiBase.Sample.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ApiBase.Infra.Options;

namespace ApiBase.Tests.Integration
{
    /// <summary>
    /// Runs ApplicationGuid (the read side) against SQLite: filters, sorts and pages are translated to SQL.
    /// Product has a protected constructor and private setters — the entity no longer needs new().
    /// </summary>
    public class QueryPipelineTests
    {
        private static ProductApplication CreateApplication(IServiceScope scope)
        {
            return new ProductApplication(
                scope.ServiceProvider.GetRequiredService<IUnitOfWork>(),
                scope.ServiceProvider.GetRequiredService<IRepositoryBase<Product>>(),
                NullLogger<ProductApplication>.Instance,
                scope.ServiceProvider.GetRequiredService<IOptions<ApiBaseOptions>>());
        }

        [Fact]
        public async Task GetAsync_FilterSortAndPage_RunsInTheDatabase()
        {
            using var services = new SqliteServices();
            using var scope = services.Provider.CreateScope();

            var result = await CreateApplication(scope).GetAsync(new QueryParams
            {
                Filter = """[{"property":"Category.Name","value":"Vegetable"}]""",
                Sort = """[{"property":"Price","direction":"desc"}]""",
                Page = 1,
                Limit = 2
            });

            Assert.Equal(3, result.Total);
            var names = result.Content.Cast<ProductView>().Select(p => p.Name).ToList();
            Assert.Equal(["Endive", "Daikon"], names);
        }

        [Fact]
        public async Task GetAsync_InFilterFromQueryString_Works()
        {
            using var services = new SqliteServices();
            using var scope = services.Provider.CreateScope();

            var result = await CreateApplication(scope).GetAsync(new QueryParams
            {
                Filter = """[{"property":"Name","operator":"in","value":["Apple","Carrot"]}]"""
            });

            Assert.Equal(2, result.Total);
        }

        [Fact]
        public async Task GetAsync_LimitAboveMaxPageSize_IsCapped()
        {
            using var services = new SqliteServices(maxPageSize: 4);
            using var scope = services.Provider.CreateScope();

            var result = await CreateApplication(scope).GetAsync(new QueryParams { Limit = 10_000 });

            Assert.Equal(6, result.Total);
            Assert.Equal(4, result.Content.Count);
        }

        [Fact]
        public async Task GetAsync_FilterOnPropertyOutsideTheView_IsRejected()
        {
            using var services = new SqliteServices();
            using var scope = services.Provider.CreateScope();

            // CostPrice exists on the entity but not on ProductView: it must not be probed through filters.
            var ex = await Assert.ThrowsAsync<InvalidQueryException>(() => CreateApplication(scope).GetAsync(new QueryParams
            {
                Filter = """[{"property":"CostPrice","operator":"greater","value":"1"}]"""
            }));

            Assert.Contains("CostPrice", ex.Message);
        }

        [Fact]
        public async Task GetAsync_SortOnPropertyOutsideTheView_IsRejected()
        {
            using var services = new SqliteServices();
            using var scope = services.Provider.CreateScope();

            await Assert.ThrowsAsync<InvalidQueryException>(() => CreateApplication(scope).GetAsync(new QueryParams
            {
                Sort = """[{"property":"CostPrice","direction":"asc"}]"""
            }));
        }

        [Fact]
        public async Task GetAsync_InvalidValue_IsRejectedInsteadOfIgnored()
        {
            using var services = new SqliteServices();
            using var scope = services.Provider.CreateScope();

            await Assert.ThrowsAsync<InvalidQueryException>(() => CreateApplication(scope).GetAsync(new QueryParams
            {
                Filter = """[{"property":"Price","value":"abc"}]"""
            }));
        }

        [Fact]
        public async Task GetAsync_ById_NotFound_ReturnsNull()
        {
            using var services = new SqliteServices();
            using var scope = services.Provider.CreateScope();

            Assert.Null(await CreateApplication(scope).GetAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetAsync_ByIdWithFields_ReturnsOnlyTheRequestedFields()
        {
            using var services = new SqliteServices();
            using var scope = services.Provider.CreateScope();
            var application = CreateApplication(scope);
            var first = (await application.GetAsync(new QueryParams { Limit = 1 })).Content.Cast<ProductView>().Single();

            var result = await application.GetAsync(first.Id, ["Name", "CostPrice"]);

            Assert.NotNull(result);
            var properties = result.GetType().GetProperties().Select(p => p.Name).ToList();
            Assert.Equal(["Name"], properties);
        }
    }
}
