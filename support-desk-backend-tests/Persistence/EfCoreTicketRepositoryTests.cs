using Microsoft.Extensions.DependencyInjection;
using SupportDesk.Api.Domain;
using SupportDesk.Api.Services;
using SupportDesk.Api.Tests.TestSupport;

namespace SupportDesk.Api.Tests.Persistence;

/// <summary>
/// Tests the EF Core repository against real SQLite: behaviour that a fake repository or EF's
/// non-relational InMemory provider would not reproduce (LIKE escaping, concurrency tokens).
/// Each test gets its own database.
/// </summary>
public sealed class EfCoreTicketRepositoryTests : IAsyncLifetime
{
    private readonly TicketApiFactory _factory = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _factory.DisposeAsync().AsTask();

    [Fact]
    public async Task UpdateAsync_WhenTicketChangedAfterItWasLoaded_ThrowsConcurrencyException()
    {
        await using var firstScope = _factory.Services.CreateAsyncScope();
        await using var secondScope = _factory.Services.CreateAsyncScope();
        var firstRepository = firstScope.ServiceProvider.GetRequiredService<ITicketRepository>();
        var secondRepository = secondScope.ServiceProvider.GetRequiredService<ITicketRepository>();
        var ticketId = (await SearchAsync(firstRepository, search: null)).First().Id;

        // Both requests load the same version, as two managers opening the ticket at once would.
        var firstCopy = (await firstRepository.GetByIdAsync(ticketId, CancellationToken.None))!;
        var secondCopy = (await secondRepository.GetByIdAsync(ticketId, CancellationToken.None))!;
        Rename(firstCopy, "First save", minutesLater: 1);
        await firstRepository.UpdateAsync(firstCopy, CancellationToken.None);
        Rename(secondCopy, "Second save", minutesLater: 2);

        await Assert.ThrowsAsync<TicketConcurrencyException>(
            () => secondRepository.UpdateAsync(secondCopy, CancellationToken.None));
    }

    [Fact]
    public async Task GetPageAsync_TreatsPercentAndUnderscoreInSearchAsLiteralText()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<ITicketRepository>();
        // Each pair differs only where an unescaped LIKE wildcard would match anything.
        foreach (var title in new[] { "Refund 50% of the invoice", "Refund 500 credits", "Order_42 is missing", "OrderX42 is missing" })
        {
            await repository.AddAsync(
                Ticket.Create(title, "Created by test", "Test Customer", null, TicketStatus.Open, TicketPriority.Low, DateTimeOffset.UtcNow),
                CancellationToken.None);
        }

        Assert.Equal(["Refund 50% of the invoice"], (await SearchAsync(repository, "50%")).Select(ticket => ticket.Title));
        Assert.Equal(["Order_42 is missing"], (await SearchAsync(repository, "order_42")).Select(ticket => ticket.Title));
    }

    private static async Task<IReadOnlyList<Ticket>> SearchAsync(ITicketRepository repository, string? search)
    {
        var page = await repository.GetPageAsync(
            new TicketListQuery(null, null, search, TicketSortField.CreatedAt, SortDescending: true, Page: 1, PageSize: 50),
            CancellationToken.None);
        return page.Tickets;
    }

    private static void Rename(Ticket ticket, string title, int minutesLater)
    {
        ticket.UpdateDetails(
            title,
            ticket.Description,
            ticket.CustomerName,
            ticket.CustomerEmail,
            ticket.Priority,
            ticket.UpdatedAt.AddMinutes(minutesLater));
    }
}
