using SupportDesk.Api.Domain;

namespace SupportDesk.Api.Services;

/// <summary>
/// Persistence port owned by the application layer: the service defines what it needs, and
/// infrastructure (<c>Repositories/EfCoreTicketRepository</c>) implements it. Dependencies point inward.
/// </summary>
public interface ITicketRepository
{
    Task<TicketPage> GetPageAsync(TicketListQuery query, CancellationToken cancellationToken);

    Task<TicketSummary> GetSummaryAsync(CancellationToken cancellationToken);

    Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(Ticket ticket, CancellationToken cancellationToken);

    /// <exception cref="TicketConcurrencyException">The ticket changed since it was loaded.</exception>
    Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken);

    /// <exception cref="TicketConcurrencyException">The ticket changed since it was loaded.</exception>
    Task DeleteAsync(Ticket ticket, CancellationToken cancellationToken);
}
