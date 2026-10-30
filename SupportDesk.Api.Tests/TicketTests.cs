using SupportDesk.Api.Domain;

namespace SupportDesk.Api.Tests;

// The aggregate's rules are plain C#, so they are tested without repositories or a clock.
public sealed class TicketTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 10, 1, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Later = CreatedAt.AddHours(3);

    [Fact]
    public void ChangeStatus_ToCurrentStatus_DoesNothing()
    {
        var ticket = CreateTicket();

        var changed = ticket.ChangeStatus(TicketStatus.Open, "Agent", Later);

        Assert.False(changed);
        Assert.Empty(ticket.StatusChanges);
        Assert.Equal(CreatedAt, ticket.UpdatedAt);
    }

    [Fact]
    public void UpdateDetails_WithEquivalentValues_ReportsNoChange()
    {
        var ticket = CreateTicket();

        // Surrounding whitespace and blank optionals normalize to the stored values.
        var changed = ticket.UpdateDetails("  Login fails  ", "Cannot sign in ", "Avery Morgan", "   ", TicketPriority.Medium, Later);

        Assert.False(changed);
        Assert.Equal(CreatedAt, ticket.UpdatedAt);
    }

    [Fact]
    public void AddComment_TrimsTextLinksTicketAndMovesUpdatedAt()
    {
        var ticket = CreateTicket();

        var comment = ticket.AddComment("  Investigating.  ", "Agent", Later);

        Assert.Equal("Investigating.", comment.Text);
        Assert.Equal(ticket.Id, comment.TicketId);
        Assert.NotEqual(Guid.Empty, comment.Id);
        Assert.Equal(Later, ticket.UpdatedAt);
    }

    private static Ticket CreateTicket()
    {
        return Ticket.Create(
            "Login fails",
            "Cannot sign in",
            "Avery Morgan",
            null,
            TicketStatus.Open,
            TicketPriority.Medium,
            CreatedAt);
    }
}
