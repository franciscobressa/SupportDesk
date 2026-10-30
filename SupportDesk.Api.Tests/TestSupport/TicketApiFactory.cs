using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SupportDesk.Api.Data;

namespace SupportDesk.Api.Tests.TestSupport;

/// <summary>
/// Hosts the real API pipeline against a private in-memory SQLite database. The connection stays
/// open for the factory's lifetime because an in-memory SQLite database disappears when it closes.
/// Each factory instance is an independent database, migrated and seeded on startup.
/// </summary>
public sealed class TicketApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public TicketApiFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<SupportDeskDbContext>>();
            services.RemoveAll<SupportDeskDbContext>();
            services.AddDbContext<SupportDeskDbContext>(options => options.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
