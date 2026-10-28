using EventTicketBookingService.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

namespace EventTicketBookingService.IntegrationTests
{
    public class PostgresContainerFixture : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("eventapi")
            .Build();

        public async Task InitializeAsync() => await _container.StartAsync();

        public async Task DisposeAsync() => await _container.DisposeAsync();

        public AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(_container.GetConnectionString())
                .Options;
            var context = new AppDbContext(options);
            return context;
        }

        public async Task ResetDatabaseAsync()
        {
            NpgsqlConnection.ClearAllPools();
            await using var context = CreateContext();
            await context.Database.EnsureDeletedAsync();
            await context.Database.MigrateAsync();
        }
    }

    [CollectionDefinition(Name)]
    public class PostgresCollection : ICollectionFixture<PostgresContainerFixture>
    {
        public const string Name = "Postgres";
    }
}
