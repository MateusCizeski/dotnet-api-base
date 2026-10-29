using ApiBase.Repository.Extensions;
using ApiBase.Sample.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ApiBase.Tests.Integration
{
    /// <summary>
    /// Builds a real service provider with AddApiBaseCore over an in-memory SQLite database,
    /// so EF Core translates the queries to SQL as in production.
    /// The connection stays open for the lifetime of the fixture (closing it drops the database).
    /// </summary>
    public sealed class SqliteServices : IDisposable
    {
        private readonly SqliteConnection _connection;

        public ServiceProvider Provider { get; }

        public SqliteServices(int maxPageSize = 100)
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddApiBaseCore<SampleDbContext>(
                options => options.UseSqlite(_connection),
                queryOptions => queryOptions.MaxPageSize = maxPageSize);

            Provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

            using var scope = Provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SampleDbContext>();
            db.Database.EnsureCreated();
            SampleData.Seed(db);
        }

        public void Dispose()
        {
            Provider.Dispose();
            _connection.Dispose();
        }
    }
}
