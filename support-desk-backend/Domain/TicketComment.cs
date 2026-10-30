namespace SupportDesk.Api.Domain;

/// <summary>A comment on a ticket; created only through <see cref="Ticket.AddComment"/>.</summary>
public sealed class TicketComment
{
    // Used by EF Core when materializing rows.
    private TicketComment()
    {
    }

    public Guid Id { get; private set; }

    public Guid TicketId { get; private set; }

    public string Text { get; private set; } = string.Empty;

    public string Author { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public Ticket Ticket { get; private set; } = null!;

    internal static TicketComment Create(Guid ticketId, string text, string author, DateTimeOffset createdAt)
    {
        return new TicketComment
        {
            Id = Guid.CreateVersion7(createdAt),
            TicketId = ticketId,
            Text = text,
            Author = author,
            CreatedAt = createdAt
        };
    }
}
