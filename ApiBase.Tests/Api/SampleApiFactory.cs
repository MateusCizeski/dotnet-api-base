using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace ApiBase.Tests.Api
{
    /// <summary>
    /// Hosts the sample API in memory with its own temporary SQLite file.
    /// </summary>
    public class SampleApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"apibase-tests-{Guid.NewGuid():N}.db");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:Default", $"Data Source={_databasePath}");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            SqliteConnection.ClearAllPools();

            if (File.Exists(_databasePath))
            {
                File.Delete(_databasePath);
            }
        }
    }
}
