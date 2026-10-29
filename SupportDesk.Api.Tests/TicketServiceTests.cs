using SupportDesk.Api.Domain;
using SupportDesk.Api.Repositories;
using SupportDesk.Api.Services;

namespace SupportDesk.Api.Tests;

public sealed class TicketServiceTests
{
    private static readonly DateTimeOffset FixedNow =
        new(2026, 10, 29, 14, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetTicketsAsync_AppliesCombinedFiltersAndCaseInsensitiveSearch()
    {
        var matchingTicket = CreateTicket(
            id: "00000000-0000-0000-0000-000000000001",
            title: "ALPHA password reset",
            status: TicketStatus.Open,
            priority: TicketPriority.High,
            createdAt: FixedNow.AddDays(-1));
        matchingTicket.CustomerEmail = "customer@example.com";

        var repository = new FakeTicketRepository(
            matchingTicket,
            CreateTicket(
                id: "00000000-0000-0000-0000-000000000002",
                title: "Alpha billing issue",
                status: TicketStatus.Open,
                priority: TicketPriority.Low,
                createdAt: FixedNow),
            CreateTicket(
                id: "00000000-0000-0000-0000-000000000003",
                title: "Alpha another request",
                status: TicketStatus.InProgress,
                priority: TicketPriority.High,
                createdAt: FixedNow));
        var service = CreateService(repository);

        var result = await service.GetTicketsAsync(
            new TicketListQuery(
                TicketStatus.Open,
                TicketPriority.High,
                "alpha",
                TicketSortField.CreatedAt,
                SortDescending: true),
            CancellationToken.None);

        Assert.Equal([matchingTicket], result);
    }

    [Theory]
    [InlineData(TicketSortField.CreatedAt, false, "00000000-0000-0000-0000-000000000001", "00000000-0000-0000-0000-000000000002", "00000000-0000-0000-0000-000000000003")]
    [InlineData(TicketSortField.CreatedAt, true, "00000000-0000-0000-0000-000000000003", "00000000-0000-0000-0000-000000000002", "00000000-0000-0000-0000-000000000001")]
    [InlineData(TicketSortField.Priority, false, "00000000-0000-0000-0000-000000000001", "00000000-0000-0000-0000-000000000002", "00000000-0000-0000-0000-000000000003")]
    [InlineData(TicketSortField.Priority, true, "00000000-0000-0000-0000-000000000003", "00000000-0000-0000-0000-000000000002", "00000000-0000-0000-0000-000000000001")]
    [InlineData(TicketSortField.Status, false, "00000000-0000-0000-0000-000000000001", "00000000-0000-0000-0000-000000000002", "00000000-0000-0000-0000-000000000003")]
    [InlineData(TicketSortField.Status, true, "00000000-0000-0000-0000-000000000003", "00000000-0000-0000-0000-000000000002", "00000000-0000-0000-0000-000000000001")]
    public async Task GetTicketsAsync_SortsBySelectedFieldAndDirection(
        TicketSortField sortBy,
        bool descending,
        string firstId,
        string secondId,
        string thirdId)
    {
        var oldest = CreateTicket(
            "00000000-0000-0000-0000-000000000001",
            "Ticket A",
            TicketStatus.Open,
            TicketPriority.Low,
            FixedNow.AddDays(-3));
        var middle = CreateTicket(
            "00000000-0000-0000-0000-000000000002",
            "Ticket B",
            TicketStatus.InProgress,
            TicketPriority.Medium,
            FixedNow.AddDays(-2));
        var newest = CreateTicket(
            "00000000-0000-0000-0000-000000000003",
            "Ticket C",
            TicketStatus.Resolved,
            TicketPriority.High,
            FixedNow.AddDays(-1));
        var service = CreateService(new FakeTicketRepository(oldest, middle, newest));

        var result = await service.GetTicketsAsync(
            new TicketListQuery(null, null, null, sortBy, descending),
            CancellationToken.None);

        Assert.Equal([firstId, secondId, thirdId], result.Select(ticket => ticket.Id.ToString()));
    }

    [Fact]
    public async Task GetTicketsAsync_UsesStableTieBreakers()
    {
        var later = CreateTicket(
            "00000000-0000-0000-0000-000000000002",
            "Later",
            TicketStatus.Open,
            TicketPriority.High,
            FixedNow);
        var earlierId = CreateTicket(
            "00000000-0000-0000-0000-000000000001",
            "Earlier ID",
            TicketStatus.Open,
            TicketPriority.High,
            FixedNow);
        var older = CreateTicket(
            "00000000-0000-0000-0000-000000000003",
            "Older",
            TicketStatus.Open,
            TicketPriority.High,
            FixedNow.AddDays(-1));
        var service = CreateService(new FakeTicketRepository(later, older, earlierId));

        var result = await service.GetTicketsAsync(
            new TicketListQuery(null, null, null, TicketSortField.Priority, SortDescending: false),
            CancellationToken.None);

        Assert.Equal([earlierId.Id, later.Id, older.Id], result.Select(ticket => ticket.Id));
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

        public Task<IReadOnlyList<Ticket>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Ticket>>(Tickets);
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
