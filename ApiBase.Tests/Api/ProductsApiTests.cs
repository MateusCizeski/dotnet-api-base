using ApiBase.Domain.Query;
using ApiBase.Infra.Query;
using ApiBase.Sample.Application;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;

namespace ApiBase.Tests.Api
{
    /// <summary>
    /// End-to-end tests over HTTP: routing, model binding, the exception handler and the JSON contract.
    /// </summary>
    public class ProductsApiTests : IClassFixture<SampleApiFactory>
    {
        private readonly SampleApiFactory _factory;

        public ProductsApiTests(SampleApiFactory factory)
        {
            _factory = factory;
        }

        private static string Query(string json) => Uri.EscapeDataString(json);

        [Fact]
        public async Task GetList_ReturnsCamelCaseViewsAndTotal()
        {
            var response = await _factory.CreateClient().GetAsync("/api/products?sort=" + Query("""[{"property":"Name","direction":"asc"}]"""));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var first = json.RootElement.GetProperty("content")[0];

            Assert.True(json.RootElement.GetProperty("total").GetInt32() >= 5);
            Assert.Equal("Apple", first.GetProperty("name").GetString());
            Assert.Equal("Fruit", first.GetProperty("category").GetProperty("name").GetString());
            Assert.False(first.TryGetProperty("costPrice", out _));
        }

        [Fact]
        public async Task GetList_WithFields_ReturnsOnlyThoseFields()
        {
            var response = await _factory.CreateClient().GetAsync("/api/products?fields=Id,Name&limit=1");

            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var names = json.RootElement.GetProperty("content")[0].EnumerateObject().Select(p => p.Name).ToList();

            Assert.Equal(["id", "name"], names.OrderBy(n => n));
        }

        [Fact]
        public async Task GetList_HugeLimit_IsCappedByMaxPageSize()
        {
            var response = await _factory.CreateClient().GetAsync("/api/products?limit=1000000");

            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(json.RootElement.GetProperty("content").GetArrayLength() <= 50);
        }

        [Theory]
        [InlineData("""[{"property":"Price","value":"abc"}]""")]
        [InlineData("""[{"property":"DoesNotExist","value":"x"}]""")]
        [InlineData("""[{"property":"CostPrice","operator":"greater","value":"0"}]""")]
        [InlineData("""not json""")]
        public async Task GetList_InvalidFilter_Returns400ProblemDetails(string filter)
        {
            var response = await _factory.CreateClient().GetAsync("/api/products?filter=" + Query(filter));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        }

        [Fact]
        public async Task GetById_Unknown_Returns404()
        {
            var response = await _factory.CreateClient().GetAsync($"/api/products/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Delete_SoftDeletes_ThenGetReturns404()
        {
            var client = _factory.CreateClient();
            var list = await client.GetAsync("/api/products?filter=" + Query("""[{"property":"Name","value":"Cherry"}]"""));
            using var json = JsonDocument.Parse(await list.Content.ReadAsStringAsync());
            var id = json.RootElement.GetProperty("content")[0].GetProperty("id").GetGuid();

            var delete = await client.DeleteAsync($"/api/products/{id}");
            var get = await client.GetAsync($"/api/products/{id}");

            Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        }

        [Fact]
        public async Task UnexpectedError_Returns500WithoutInternalDetails()
        {
            var client = _factory.WithWebHostBuilder(builder =>
                builder.ConfigureTestServices(services => services.AddScoped<IProductApplication, ThrowingProductApplication>()))
                .CreateClient();

            var response = await client.GetAsync("/api/products");
            string body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.DoesNotContain(ThrowingProductApplication.Secret, body);
            Assert.DoesNotContain("   at ", body);
        }

        private sealed class ThrowingProductApplication : IProductApplication
        {
            public const string Secret = "SELECT * FROM internal_table";

            public Task<GetView> GetAsync(QueryParams queryParams, CancellationToken cancellationToken = default) => throw new InvalidOperationException(Secret);
            public Task<ProductView?> GetAsync(Guid id, CancellationToken cancellationToken = default) => throw new InvalidOperationException(Secret);
            public Task<object?> GetAsync(Guid id, List<string> fields, CancellationToken cancellationToken = default) => throw new InvalidOperationException(Secret);
            public GetView Get(QueryParams queryParams) => throw new InvalidOperationException(Secret);
            public ProductView? Get(Guid id) => throw new InvalidOperationException(Secret);
            public object? Get(Guid id, List<string> fields) => throw new InvalidOperationException(Secret);
            public Task DeleteAsync(Guid id, CancellationToken cancellationToken) => throw new InvalidOperationException(Secret);
        }
    }
}
