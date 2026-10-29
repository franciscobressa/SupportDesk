using Microsoft.EntityFrameworkCore;
using SupportDesk.Api.Data;
using Testcontainers.PostgreSql;

namespace SupportDesk.Api.Tests;

public sealed class PostgresDatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18-alpine")
        .WithDatabase("supportdesk_tests")
        .WithUsername("supportdesk_tests")
        .WithPassword("supportdesk_tests")
        .Build();

    public string ConnectionString => _postgres.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<SupportDeskDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        await using var dbContext = new SupportDeskDbContext(options);
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }
}
