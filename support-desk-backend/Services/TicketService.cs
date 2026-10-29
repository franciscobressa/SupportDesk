using SupportDesk.Api.Domain;
using SupportDesk.Api.Repositories;

namespace SupportDesk.Api.Services;

public sealed class TicketService(ITicketRepository ticketRepository, TimeProvider timeProvider)
{
    private const string MockAuthor = "Support Manager (mock)";

    public async Task<IReadOnlyList<Ticket>> GetTicketsAsync(
        TicketListQuery query,
        CancellationToken cancellationToken)
    {
        IEnumerable<Ticket> tickets = await ticketRepository.GetAllAsync(cancellationToken);

        if (query.Status is not null)
        {
            tickets = tickets.Where(ticket => ticket.Status == query.Status);
        }

        if (query.Priority is not null)
        {
            tickets = tickets.Where(ticket => ticket.Priority == query.Priority);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            tickets = tickets.Where(ticket =>
                ticket.Title.Contains(search, StringComparison.OrdinalIgnoreCase)
                || ticket.Description.Contains(search, StringComparison.OrdinalIgnoreCase)
                || (ticket.CustomerName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                || (ticket.CustomerEmail?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        return ApplyOrdering(tickets, query).ToList();
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

    private static IOrderedEnumerable<Ticket> ApplyOrdering(
        IEnumerable<Ticket> tickets,
        TicketListQuery query)
    {
        return query.SortBy switch
        {
            TicketSortField.CreatedAt when query.SortDescending =>
                tickets.OrderByDescending(ticket => ticket.CreatedAt).ThenBy(ticket => ticket.Id),
            TicketSortField.CreatedAt =>
                tickets.OrderBy(ticket => ticket.CreatedAt).ThenBy(ticket => ticket.Id),
            TicketSortField.Priority when query.SortDescending =>
                tickets.OrderByDescending(ticket => ticket.Priority)
                    .ThenByDescending(ticket => ticket.CreatedAt)
                    .ThenBy(ticket => ticket.Id),
            TicketSortField.Priority =>
                tickets.OrderBy(ticket => ticket.Priority)
                    .ThenByDescending(ticket => ticket.CreatedAt)
                    .ThenBy(ticket => ticket.Id),
            TicketSortField.Status when query.SortDescending =>
                tickets.OrderByDescending(ticket => ticket.Status)
                    .ThenByDescending(ticket => ticket.CreatedAt)
                    .ThenBy(ticket => ticket.Id),
            TicketSortField.Status =>
                tickets.OrderBy(ticket => ticket.Status)
                    .ThenByDescending(ticket => ticket.CreatedAt)
                    .ThenBy(ticket => ticket.Id),
            _ => throw new ArgumentOutOfRangeException(nameof(query.SortBy))
        };
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
