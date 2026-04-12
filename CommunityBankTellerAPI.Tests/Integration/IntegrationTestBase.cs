using Npgsql;
using Respawn;

namespace CommunityBankTellerAPI.Tests.Integration
{
    // IAsyncLifetime is an xUnit interface that provides async setup and teardown hooks.
    // xUnit automatically calls InitializeAsync() before each test and DisposeAsync() after each test.
    public abstract class IntegrationTestBase : IAsyncLifetime
    {
        // holds a reference to the factory so we can access the test DB connection string in InitializeAsync for Respawn to use
        private readonly CustomWebApplicationFactory _factory;

        // the HttpClient used to make real HTTP requests to the test API in every test.
        protected readonly HttpClient Client;

        // holds the Respawner instance used to reset the database before each test.
        private Respawner _respawner = null!;

        // JSON serialization options used for sending and receiving JSON in tests.
        protected static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
        {
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
            PropertyNameCaseInsensitive = true
        };

        // constructor receives the factory via xUnit's dependency injection.
        // stores the factory reference for later use in InitializeAsync,
        // and creates the HttpClient that tests will use to hit the API
        protected IntegrationTestBase(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            Client = factory.CreateClient();
        }

        /// <summary>
        /// Initializes the test database by configuring the respawner and resetting all data to a clean state
        /// asynchronously.
        /// </summary>
        /// <remarks>This method establishes a direct connection to the PostgreSQL database and uses
        /// Respawn to clear all data from tables in the "public" schema, ensuring a consistent starting point for
        /// tests. The database schema and migration history are preserved. This method should be called before running
        /// tests that require a clean database state.</remarks>
        /// <returns>A task that represents the asynchronous initialization operation.</returns>
        public async Task InitializeAsync()
        {
            // open a direct ADO.NET connection to the test PostgreSQL database.
            // Respawn needs a raw connection, not EF Core, because it runs its own DELETE statements
            await using var conn = new NpgsqlConnection(_factory.ConnectionString);
            await conn.OpenAsync();

            // create the Respawner instance configured for PostgreSQL.
            // Respawner inspects the DB schema at this point so it knows the table order
            // needed to delete data without violating foreign key constraints
            _respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
            {
                // tells Respawn this is a PostgreSQL database so it uses the correct SQL dialect
                DbAdapter = DbAdapter.Postgres,
                // only reset tables in the "public" schema — ignores system schemas like
                // pg_catalog, information_schema, and EF Core's __EFMigrationsHistory table
                SchemasToInclude = new[] { "public" }
            });

            // reset all data immediately before this test runs —
            // wipes all rows in the correct order to respect foreign key constraints,
            // leaving the schema and migrations completely intact
            await _respawner.ResetAsync(conn);
        }

        /// <summary>
        /// Asynchronously releases resources used by the current instance.
        /// </summary>
        /// <returns>A completed task that represents the asynchronous dispose operation.</returns>
        public Task DisposeAsync() => Task.CompletedTask;
    }
}