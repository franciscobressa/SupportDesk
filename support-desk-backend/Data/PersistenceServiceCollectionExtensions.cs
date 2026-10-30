using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SupportDesk.Api.Repositories;
using SupportDesk.Api.Services;

namespace SupportDesk.Api.Data;

public static class PersistenceServiceCollectionExtensions
{
    /// <summary>Registers the SQLite database and the EF Core implementation of the persistence port.</summary>
    public static IServiceCollection AddSupportDeskPersistence(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("SupportDesk")
            ?? throw new InvalidOperationException("The SupportDesk connection string is not configured.");

        // A relative path would depend on the shell's working directory; anchor it to the project folder.
        var sqliteConnection = new SqliteConnectionStringBuilder(connectionString);
        if (!Path.IsPathRooted(sqliteConnection.DataSource))
        {
            sqliteConnection.DataSource = Path.Combine(environment.ContentRootPath, sqliteConnection.DataSource);
        }

        var databaseDirectory = Path.GetDirectoryName(sqliteConnection.DataSource);
        if (!string.IsNullOrEmpty(databaseDirectory))
        {
            Directory.CreateDirectory(databaseDirectory);
        }

        services.AddDbContext<SupportDeskDbContext>(options => options.UseSqlite(sqliteConnection.ToString()));
        services.AddScoped<ITicketRepository, EfCoreTicketRepository>();
        return services;
    }
}
