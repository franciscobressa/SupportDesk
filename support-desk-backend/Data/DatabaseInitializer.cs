using Microsoft.EntityFrameworkCore;

namespace SupportDesk.Api.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SupportDeskDbContext>();

        await dbContext.Database.MigrateAsync(cancellationToken);

        // Seed only an empty database so tickets deleted by users are not recreated on restart.
        if (await dbContext.Tickets.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = scope.ServiceProvider.GetRequiredService<TimeProvider>().GetUtcNow();
        dbContext.Tickets.AddRange(DemoTickets.Create(now));
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
