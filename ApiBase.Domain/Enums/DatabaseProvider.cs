namespace ApiBase.Domain.Enums
{
    /// <summary>
    /// Identifies the database provider being used by the application.
    /// Used for provider-specific query or configuration logic.
    /// </summary>
    public enum DatabaseProvider
    {
        /// <summary>Oracle Database.</summary>
        Oracle = 1,

        /// <summary>PostgreSQL.</summary>
        Postgres = 2,

        /// <summary>Microsoft SQL Server.</summary>
        SqlServer = 3,

        /// <summary>MySQL.</summary>
        MySql = 4,

        /// <summary>SQLite (typically used for testing or embedded scenarios).</summary>
        Sqlite = 5,

        /// <summary>MariaDB.</summary>
        MariaDb = 6,

        /// <summary>IBM Db2.</summary>
        Db2 = 7,

        /// <summary>MongoDB (document database).</summary>
        MongoDb = 8,

        /// <summary>Azure Cosmos DB.</summary>
        CosmosDb = 9
    }
}
