namespace SupportDesk.Api.Domain;

public sealed class TicketStatusChange
{
    public Guid Id { get; set; }

    public Guid TicketId { get; set; }

    public TicketStatus PreviousStatus { get; set; }

    public TicketStatus NewStatus { get; set; }

    public string Author { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public Ticket Ticket { get; set; } = null!;
}
