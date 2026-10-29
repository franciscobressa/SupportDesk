namespace SupportDesk.Api.Domain;

public sealed class Ticket
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public TicketStatus Status { get; set; } = TicketStatus.Open;

    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    public string? CustomerName { get; set; }

    public string? CustomerEmail { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public List<TicketComment> Comments { get; set; } = [];

    public List<TicketStatusChange> StatusChanges { get; set; } = [];
}
