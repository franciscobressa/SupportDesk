using SupportDesk.Api.Domain;

namespace SupportDesk.Api.Services;

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
    bool SortDescending);
