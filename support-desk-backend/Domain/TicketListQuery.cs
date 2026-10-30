namespace SupportDesk.Api.Domain;

public enum TicketSortField
{
    CreatedAt,
    Priority,
    Status
}

public sealed record TicketListQuery(
    TicketStatus? Status,
    TicketPriority? Priority,
    string? Search,
    TicketSortField SortBy,
    bool SortDescending,
    int Page = 1,
    int PageSize = 10);

public sealed record TicketSummary(
    int Open,
    int InProgress,
    int Resolved,
    int HighPriority);

public sealed record TicketPage(
    IReadOnlyList<Ticket> Tickets,
    int TotalCount);
