namespace SupportDesk.Api.Domain;

public sealed class TicketComment
{
    public Guid Id { get; set; }

    public Guid TicketId { get; set; }

    public string Text { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public Ticket Ticket { get; set; } = null!;
}
