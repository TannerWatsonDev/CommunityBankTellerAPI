using CommunityBankTellerAPI.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommunityBankTellerAPI.Tests.Integration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        public string ConnectionString { get; private set; } = string.Empty;

        // static so it is shared across ALL instances of this factory —
        // xUnit creates a new factory instance per test class, so without static
        // this flag would reset to false for each test class and Migrate() would
        // run multiple times, causing "relation already exists" errors
        private static bool _migrated = false;

        // lock object to ensure thread safety when multiple test classes run in parallel and access the _migrated flag
        private static readonly object _migrationLock = new();

        /// <summary>
        /// Configures the web host builder to use the testing environment and test-specific configuration  for integration tests.
        /// </summary>
        /// <remarks>This method sets the environment to "Testing", loads the "appsettings.Testing.json"
        /// configuration file, and configures services to use a test database. It ensures that the test database schema is reset and migrations are applied only once per test session, providing a consistent and isolated environment for integration tests. Override this method to customize the test host configuration as needed.</remarks>
        /// <param name="builder">The <see cref="IWebHostBuilder"/> to configure for the test environment.</param>
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // set the environment to "Testing" so the app loads appsettings.Testing.json and any test-specific services or configuration
            builder.UseEnvironment("Testing");

            // add appsettings.Testing.json to the configuration so we can read the test DB connection string from it
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.Testing.json", optional: false);
            });

            // configure services to use the test database and apply migrations
            builder.ConfigureServices((context, services) =>
            {
                // read connection string from appsettings.Testing.json
                ConnectionString = context.Configuration
                    .GetConnectionString("DefaultConnection")!;

                // register AppDbContext with the test DB
                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(ConnectionString));

                // build a temporary service provider to access AppDbContext
                var sp = services.BuildServiceProvider();
                // create a scope to get a scoped AppDbContext instance
                using var scope = sp.CreateScope();
                // get the AppDbContext instance from the scope's service provider
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // lock ensures thread safety if xUnit runs test classes in parallel.
                // the static _migrated flag ensures EnsureDeleted + Migrate only runs
                // once per test session regardless of how many factory instances xUnit creates.
                // without this guard, Migrate() would run per test class and fail with
                // "relation already exists" because the tables were already created.
                lock (_migrationLock)
                {
                    if (!_migrated)
                    {
                        // wipe the test DB completely so we start from a clean slate —
                        // this prevents leftover schema or data from a previous test run
                        // from interfering with migrations
                        db.Database.EnsureDeleted();

                        // run all pending migrations against the fresh test DB.
                        // this validates that your actual migration files work correctly,
                        // unlike EnsureCreated() which bypasses migrations entirely
                        db.Database.Migrate();

                        _migrated = true;
                    }
                }
            });
        }
    }
}