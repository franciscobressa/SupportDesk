namespace SupportDesk.Api.Domain;

/// <summary>A status transition; created only through <see cref="Ticket.ChangeStatus"/>.</summary>
public sealed class TicketStatusChange
{
    // Used by EF Core when materializing rows.
    private TicketStatusChange()
    {
    }

    public Guid Id { get; private set; }

    public Guid TicketId { get; private set; }

    public TicketStatus PreviousStatus { get; private set; }

    public TicketStatus NewStatus { get; private set; }

    public string Author { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public Ticket Ticket { get; private set; } = null!;

    internal static TicketStatusChange Create(
        Guid ticketId,
        TicketStatus previousStatus,
        TicketStatus newStatus,
        string author,
        DateTimeOffset createdAt)
    {
        return new TicketStatusChange
        {
            Id = Guid.CreateVersion7(createdAt),
            TicketId = ticketId,
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            Author = author,
            CreatedAt = createdAt
        };
    }
}
