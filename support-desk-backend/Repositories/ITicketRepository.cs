using SupportDesk.Api.Domain;

namespace SupportDesk.Api.Repositories;

public interface ITicketRepository
{
    Task<(IReadOnlyList<Ticket> Tickets, int TotalCount, TicketSummary Summary)> GetPageAsync(
        TicketListQuery query,
        CancellationToken cancellationToken);

    Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(Ticket ticket, CancellationToken cancellationToken);

    Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken);

    Task DeleteAsync(Ticket ticket, CancellationToken cancellationToken);
}
