using SupportDesk.Api.Domain;

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
