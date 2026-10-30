using SupportDesk.Api.Data;
using SupportDesk.Api.Domain;

namespace SupportDesk.Api.Tests.TestSupport;

/// <summary>
/// Expected values derived from the same seed the API uses, so tests do not hard-code counts
/// that silently break whenever the demo data is edited.
/// </summary>
public static class SeedData
{
    // Any recent instant works: counts do not depend on dates. (Demo dates are relative to it and
    // must stay after 1970, since ticket ids are time-based GUID v7 values.)
    public static IReadOnlyList<Ticket> Tickets { get; } =
        DemoTickets.Create(new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero));

    public static int Count => Tickets.Count;

    public static int CountWith(TicketStatus status) => Tickets.Count(ticket => ticket.Status == status);

    public static int CountWith(TicketPriority priority) => Tickets.Count(ticket => ticket.Priority == priority);

    public static int TotalPages(int pageSize) => (int)Math.Ceiling(Count / (double)pageSize);
}
