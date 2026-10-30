namespace SupportDesk.Api.Domain;

/// <summary>One page of tickets plus the number of tickets matching the filters across all pages.</summary>
public sealed record TicketPage(
    IReadOnlyList<Ticket> Tickets,
    int TotalCount);
