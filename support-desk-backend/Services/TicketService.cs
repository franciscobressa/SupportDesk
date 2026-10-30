using SupportDesk.Api.Domain;
using SupportDesk.Api.Repositories;

namespace SupportDesk.Api.Services;

public sealed class TicketService(ITicketRepository ticketRepository, TimeProvider timeProvider)
{
    private const string MockAuthor = "Support Manager (mock)";

    public Task<TicketPage> GetTicketsAsync(TicketListQuery query, CancellationToken cancellationToken)
    {
        return ticketRepository.GetPageAsync(query, cancellationToken);
    }

    public Task<Ticket?> GetTicketAsync(Guid id, CancellationToken cancellationToken)
    {
        return ticketRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<Ticket> CreateTicketAsync(
        CreateTicketCommand command,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var ticket = new Ticket
        {
            Title = command.Title.Trim(),
            Description = command.Description.Trim(),
            CustomerName = NormalizeOptional(command.CustomerName),
            CustomerEmail = NormalizeOptional(command.CustomerEmail),
            Status = command.Status ?? TicketStatus.Open,
            Priority = command.Priority ?? TicketPriority.Medium,
            CreatedAt = now,
            UpdatedAt = now
        };

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

        var title = command.Title.Trim();
        var description = command.Description.Trim();
        var customerName = NormalizeOptional(command.CustomerName);
        var customerEmail = NormalizeOptional(command.CustomerEmail);
        var statusChanged = ticket.Status != command.Status;
        var hasChanges = statusChanged
            || ticket.Priority != command.Priority
            || ticket.Title != title
            || ticket.Description != description
            || ticket.CustomerName != customerName
            || ticket.CustomerEmail != customerEmail;

        if (!hasChanges)
        {
            return ticket;
        }

        var now = timeProvider.GetUtcNow();
        if (statusChanged)
        {
            ticket.StatusChanges.Add(new TicketStatusChange
            {
                PreviousStatus = ticket.Status,
                NewStatus = command.Status,
                Author = MockAuthor,
                CreatedAt = now
            });
        }

        ticket.Title = title;
        ticket.Description = description;
        ticket.CustomerName = customerName;
        ticket.CustomerEmail = customerEmail;
        ticket.Status = command.Status;
        ticket.Priority = command.Priority;
        ticket.UpdatedAt = now;

        await ticketRepository.UpdateAsync(ticket, cancellationToken);
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

        var now = timeProvider.GetUtcNow();
        var comment = new TicketComment
        {
            TicketId = ticketId,
            Text = text.Trim(),
            Author = MockAuthor,
            CreatedAt = now
        };

        ticket.Comments.Add(comment);
        ticket.UpdatedAt = now;
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

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
