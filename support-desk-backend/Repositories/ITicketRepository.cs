using SupportDesk.Api.Domain;

namespace SupportDesk.Api.Repositories;

public interface ITicketRepository
{
    Task<TicketPage> GetPageAsync(TicketListQuery query, CancellationToken cancellationToken);

    /// <summary>Queue-wide counts, independent of any list filter.</summary>
    Task<TicketSummary> GetSummaryAsync(CancellationToken cancellationToken);

    Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(Ticket ticket, CancellationToken cancellationToken);

    /// <exception cref="TicketConcurrencyException">The ticket changed since it was loaded.</exception>
    Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken);

    /// <exception cref="TicketConcurrencyException">The ticket changed since it was loaded.</exception>
    Task DeleteAsync(Ticket ticket, CancellationToken cancellationToken);
}
