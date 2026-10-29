using SupportDesk.Api.Domain;
using SupportDesk.Api.Repositories;
using SupportDesk.Api.Services;

namespace SupportDesk.Api.Tests;

public sealed class TicketServiceTests
{
    private static readonly DateTimeOffset FixedNow =
        new(2026, 10, 29, 14, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetTicketsAsync_ForwardsQueryToRepository()
    {
        var ticket = CreateTicket(
            "00000000-0000-0000-0000-000000000001",
            "Requested page",
            TicketStatus.Open,
            TicketPriority.High,
            FixedNow);
        var repository = new FakeTicketRepository(ticket);
        var service = CreateService(repository);
        var query = new TicketListQuery(
            TicketStatus.Open,
            TicketPriority.High,
            "password",
            TicketSortField.CreatedAt,
            SortDescending: true,
            Page: 2,
            PageSize: 10);

        var result = await service.GetTicketsAsync(
            query,
            CancellationToken.None);

        Assert.Equal(query, repository.LastListQuery);
        Assert.Equal([ticket], result.Tickets);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task CreateTicketAsync_AppliesDefaultsNormalizesOptionalValuesAndUsesUtcTime()
    {
        var repository = new FakeTicketRepository();
        var service = CreateService(repository);

        var ticket = await service.CreateTicketAsync(
            new CreateTicketCommand(
                "  Password reset  ",
                "  Customer cannot sign in.  ",
                "  Avery Morgan  ",
                "  ",
                Status: null,
                Priority: null),
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, ticket.Id);
        Assert.Equal("Password reset", ticket.Title);
        Assert.Equal("Customer cannot sign in.", ticket.Description);
        Assert.Equal("Avery Morgan", ticket.CustomerName);
        Assert.Null(ticket.CustomerEmail);
        Assert.Equal(TicketStatus.Open, ticket.Status);
        Assert.Equal(TicketPriority.Medium, ticket.Priority);
        Assert.Equal(FixedNow, ticket.CreatedAt);
        Assert.Equal(FixedNow, ticket.UpdatedAt);
        Assert.Equal(ticket, Assert.Single(repository.Tickets));
    }

    [Fact]
    public async Task UpdateTicketAsync_DoesNotSaveOrChangeTimestampWhenNothingChanged()
    {
        var ticket = CreateTicket(
            "00000000-0000-0000-0000-000000000001",
            "Existing title",
            TicketStatus.Open,
            TicketPriority.Medium,
            FixedNow.AddDays(-1));
        ticket.Description = "Existing description";
        ticket.CustomerName = "Avery Morgan";
        var repository = new FakeTicketRepository(ticket);
        var service = CreateService(repository);

        var result = await service.UpdateTicketAsync(
            ticket.Id,
            new UpdateTicketCommand(
                ticket.Title,
                ticket.Description,
                ticket.CustomerName,
                ticket.CustomerEmail,
                ticket.Status,
                ticket.Priority),
            CancellationToken.None);

        Assert.Same(ticket, result);
        Assert.Equal(FixedNow.AddDays(-1), ticket.UpdatedAt);
        Assert.Empty(ticket.StatusChanges);
        Assert.Equal(0, repository.UpdateCount);
    }

    [Fact]
    public async Task UpdateTicketAsync_RecordsStatusChangeWithPreviousAndNewValues()
    {
        var ticket = CreateTicket(
            "00000000-0000-0000-0000-000000000001",
            "Existing title",
            TicketStatus.Open,
            TicketPriority.Medium,
            FixedNow.AddDays(-1));
        ticket.Description = "Existing description";
        var repository = new FakeTicketRepository(ticket);
        var service = CreateService(repository);

        var result = await service.UpdateTicketAsync(
            ticket.Id,
            new UpdateTicketCommand(
                ticket.Title,
                ticket.Description,
                ticket.CustomerName,
                ticket.CustomerEmail,
                TicketStatus.InProgress,
                ticket.Priority),
            CancellationToken.None);

        var change = Assert.Single(Assert.IsType<Ticket>(result).StatusChanges);
        Assert.Equal(TicketStatus.Open, change.PreviousStatus);
        Assert.Equal(TicketStatus.InProgress, change.NewStatus);
        Assert.Equal("Support Manager (mock)", change.Author);
        Assert.Equal(FixedNow, change.CreatedAt);
        Assert.Equal(FixedNow, result.UpdatedAt);
        Assert.Equal(1, repository.UpdateCount);
    }

    [Fact]
    public async Task UpdateTicketAsync_ReturnsNullWhenTicketDoesNotExist()
    {
        var service = CreateService(new FakeTicketRepository());

        var result = await service.UpdateTicketAsync(
            Guid.NewGuid(),
            new UpdateTicketCommand(
                "Title",
                "Description",
                "Customer",
                null,
                TicketStatus.Open,
                TicketPriority.Low),
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task AddCommentAsync_UsesServerAuthorAndTimeAndUpdatesTicket()
    {
        var ticket = CreateTicket(
            "00000000-0000-0000-0000-000000000001",
            "Existing title",
            TicketStatus.Open,
            TicketPriority.Medium,
            FixedNow.AddDays(-1));
        var repository = new FakeTicketRepository(ticket);
        var service = CreateService(repository);

        var comment = await service.AddCommentAsync(ticket.Id, "  Follow-up sent.  ", CancellationToken.None);

        Assert.NotNull(comment);
        Assert.Equal("Follow-up sent.", comment.Text);
        Assert.Equal("Support Manager (mock)", comment.Author);
        Assert.Equal(FixedNow, comment.CreatedAt);
        Assert.Equal(FixedNow, ticket.UpdatedAt);
        Assert.Same(comment, Assert.Single(ticket.Comments));
        Assert.Equal(1, repository.UpdateCount);
    }

    [Fact]
    public async Task DeleteTicketAsync_RemovesExistingTicketAndReturnsFalseForMissingTicket()
    {
        var ticket = CreateTicket(
            "00000000-0000-0000-0000-000000000001",
            "Existing title",
            TicketStatus.Open,
            TicketPriority.Medium,
            FixedNow);
        var repository = new FakeTicketRepository(ticket);
        var service = CreateService(repository);

        Assert.True(await service.DeleteTicketAsync(ticket.Id, CancellationToken.None));
        Assert.Empty(repository.Tickets);
        Assert.False(await service.DeleteTicketAsync(ticket.Id, CancellationToken.None));
    }

    private static TicketService CreateService(FakeTicketRepository repository)
    {
        return new TicketService(repository, new FixedTimeProvider(FixedNow));
    }

    private static Ticket CreateTicket(
        string id,
        string title,
        TicketStatus status,
        TicketPriority priority,
        DateTimeOffset createdAt)
    {
        return new Ticket
        {
            Id = Guid.Parse(id),
            Title = title,
            Description = title,
            Status = status,
            Priority = priority,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };
    }

    private sealed class FakeTicketRepository(params Ticket[] tickets) : ITicketRepository
    {
        public List<Ticket> Tickets { get; } = [.. tickets];

        public int UpdateCount { get; private set; }

        public TicketListQuery? LastListQuery { get; private set; }

        public Task<(IReadOnlyList<Ticket> Tickets, int TotalCount)> GetPageAsync(
            TicketListQuery query,
            CancellationToken cancellationToken)
        {
            LastListQuery = query;
            return Task.FromResult(((IReadOnlyList<Ticket>)Tickets, Tickets.Count));
        }

        public Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(Tickets.SingleOrDefault(ticket => ticket.Id == id));
        }

        public Task AddAsync(Ticket ticket, CancellationToken cancellationToken)
        {
            if (ticket.Id == Guid.Empty)
            {
                ticket.Id = Guid.NewGuid();
            }

            Tickets.Add(ticket);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken)
        {
            UpdateCount++;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Ticket ticket, CancellationToken cancellationToken)
        {
            Tickets.Remove(ticket);
            return Task.CompletedTask;
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
