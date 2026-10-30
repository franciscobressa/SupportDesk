using SupportDesk.Api.Domain;
using SupportDesk.Api.Repositories;

namespace SupportDesk.Api.Services;

/// <summary>
/// Application layer: loads tickets, applies use-case checks, delegates state changes to the
/// <see cref="Ticket"/> aggregate and persists the result.
/// </summary>
public sealed class TicketService(ITicketRepository ticketRepository, TimeProvider timeProvider)
{
    public Task<TicketPage> GetTicketsAsync(TicketListQuery query, CancellationToken cancellationToken)
    {
        return ticketRepository.GetPageAsync(query, cancellationToken);
    }

    public Task<TicketSummary> GetSummaryAsync(CancellationToken cancellationToken)
    {
        return ticketRepository.GetSummaryAsync(cancellationToken);
    }

    public Task<Ticket?> GetTicketAsync(Guid id, CancellationToken cancellationToken)
    {
        return ticketRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Ticket> CreateTicketAsync(
        CreateTicketCommand command,
        CancellationToken cancellationToken)
    {
        var ticket = Ticket.Create(
            command.Title,
            command.Description,
            command.CustomerName,
            command.CustomerEmail,
            command.Status ?? TicketStatus.Open,
            command.Priority ?? TicketPriority.Medium,
            timeProvider.GetUtcNow());

        await ticketRepository.AddAsync(ticket, cancellationToken);
        return ticket;
    }

    public async Task<Ticket?> UpdateTicketAsync(
        Guid id,
        UpdateTicketCommand command,
        CancellationToken cancellationToken)
    {
        var ticket = await ticketRepository.GetByIdAsync(id, cancellationToken);
        if (ticket is null)
        {
            return null;
        }

        // The client edited a stale copy: reject instead of overwriting the newer version.
        // The repository's concurrency token also covers a write that lands between this check and the save.
        if (ticket.UpdatedAt != command.ExpectedUpdatedAt)
        {
            throw new TicketConcurrencyException(ticket.Id);
        }

        var now = timeProvider.GetUtcNow();
        var detailsChanged = ticket.UpdateDetails(
            command.Title,
            command.Description,
            command.CustomerName,
            command.CustomerEmail,
            command.Priority,
            now);
        var statusChanged = ticket.ChangeStatus(command.Status, TicketConstraints.MockAuthor, now);

        if (detailsChanged || statusChanged)
        {
            await ticketRepository.UpdateAsync(ticket, cancellationToken);
        }

        return ticket;
    }

    public async Task<TicketComment?> AddCommentAsync(
        Guid ticketId,
        string text,
        CancellationToken cancellationToken)
    {
        var ticket = await ticketRepository.GetByIdAsync(ticketId, cancellationToken);
        if (ticket is null)
        {
            return null;
        }

        var comment = ticket.AddComment(text, TicketConstraints.MockAuthor, timeProvider.GetUtcNow());
        await ticketRepository.UpdateAsync(ticket, cancellationToken);
        return comment;
    }

    public async Task<bool> DeleteTicketAsync(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await ticketRepository.GetByIdAsync(id, cancellationToken);
        if (ticket is null)
        {
            return false;
        }

        await ticketRepository.DeleteAsync(ticket, cancellationToken);
        return true;
    }
}
