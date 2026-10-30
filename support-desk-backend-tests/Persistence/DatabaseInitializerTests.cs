using Microsoft.Extensions.DependencyInjection;
using SupportDesk.Api.Data;
using SupportDesk.Api.Domain;
using SupportDesk.Api.Services;
using SupportDesk.Api.Tests.TestSupport;

namespace SupportDesk.Api.Tests.Persistence;

public sealed class DatabaseInitializerTests : IAsyncLifetime
{
    private readonly TicketApiFactory _factory = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _factory.DisposeAsync().AsTask();

    [Fact]
    public async Task InitializeAsync_OnRestart_DoesNotRecreateDeletedDemoTickets()
    {
        Guid deletedTicketId;
        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<ITicketRepository>();
            var ticket = (await repository.GetPageAsync(FirstPage, CancellationToken.None)).Tickets[0];
            deletedTicketId = ticket.Id;
            await repository.DeleteAsync(ticket, CancellationToken.None);
        }

        // Simulates the next application start against the same database.
        await DatabaseInitializer.InitializeAsync(_factory.Services);

        await using var verifyScope = _factory.Services.CreateAsyncScope();
        var verifyRepository = verifyScope.ServiceProvider.GetRequiredService<ITicketRepository>();
        Assert.Equal(SeedData.Count - 1, (await verifyRepository.GetPageAsync(FirstPage, CancellationToken.None)).TotalCount);
        Assert.Null(await verifyRepository.GetByIdAsync(deletedTicketId, CancellationToken.None));
    }

    private static TicketListQuery FirstPage =>
        new(null, null, null, TicketSortField.CreatedAt, SortDescending: true, Page: 1, PageSize: 10);
}
