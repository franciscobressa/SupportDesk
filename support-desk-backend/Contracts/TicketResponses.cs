using SupportDesk.Api.Domain;

namespace SupportDesk.Api.Contracts;

public sealed record TicketListItemResponse(
    Guid Id,
    string Title,
    string Description,
    TicketStatus Status,
    TicketPriority Priority,
    string? CustomerName,
    string? CustomerEmail,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record TicketListResponse(
    IReadOnlyList<TicketListItemResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    TicketSummary Summary);

public sealed record TicketCommentResponse(
    Guid Id,
    string Text,
    string Author,
    DateTimeOffset CreatedAt);

public sealed record TicketStatusChangeResponse(
    Guid Id,
    TicketStatus PreviousStatus,
    TicketStatus NewStatus,
    string Author,
    DateTimeOffset CreatedAt);

public sealed record TicketDetailsResponse(
    Guid Id,
    string Title,
    string Description,
    TicketStatus Status,
    TicketPriority Priority,
    string? CustomerName,
    string? CustomerEmail,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<TicketCommentResponse> Comments,
    IReadOnlyList<TicketStatusChangeResponse> StatusChanges)
{
    public static TicketDetailsResponse FromTicket(Ticket ticket)
    {
        return new TicketDetailsResponse(
            ticket.Id,
            ticket.Title,
            ticket.Description,
            ticket.Status,
            ticket.Priority,
            ticket.CustomerName,
            ticket.CustomerEmail,
            ticket.CreatedAt,
            ticket.UpdatedAt,
            ticket.Comments
                .OrderBy(comment => comment.CreatedAt)
                .Select(comment => new TicketCommentResponse(
                    comment.Id,
                    comment.Text,
                    comment.Author,
                    comment.CreatedAt))
                .ToArray(),
            ticket.StatusChanges
                .OrderBy(change => change.CreatedAt)
                .Select(change => new TicketStatusChangeResponse(
                    change.Id,
                    change.PreviousStatus,
                    change.NewStatus,
                    change.Author,
                    change.CreatedAt))
                .ToArray());
    }
}
