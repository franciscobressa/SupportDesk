using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SupportDesk.Api.Data;
using SupportDesk.Api.Domain;
using SupportDesk.Api.Services;

namespace SupportDesk.Api.Repositories;

public sealed class EfCoreTicketRepository(SupportDeskDbContext dbContext) : ITicketRepository
{
    // Enums are stored as strings for readability, so their domain order is expressed as SQL CASE ranks.
    private static readonly Expression<Func<Ticket, int>> PriorityRank = ticket =>
        ticket.Priority == TicketPriority.High ? 3
        : ticket.Priority == TicketPriority.Medium ? 2
        : 1;

    private static readonly Expression<Func<Ticket, int>> StatusRank = ticket =>
        ticket.Status == TicketStatus.Closed ? 4
        : ticket.Status == TicketStatus.Resolved ? 3
        : ticket.Status == TicketStatus.InProgress ? 2
        : 1;

    public async Task<TicketPage> GetPageAsync(TicketListQuery query, CancellationToken cancellationToken)
    {
        IQueryable<Ticket> tickets = dbContext.Tickets.AsNoTracking();

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
            var search = EscapeLikePattern(query.Search.Trim());
            var pattern = $"%{search}%";
            tickets = tickets.Where(ticket =>
                EF.Functions.Like(ticket.Title, pattern, "\\")
                || EF.Functions.Like(ticket.Description, pattern, "\\")
                || (ticket.CustomerName != null && EF.Functions.Like(ticket.CustomerName, pattern, "\\"))
                || (ticket.CustomerEmail != null && EF.Functions.Like(ticket.CustomerEmail, pattern, "\\")));
        }

        var totalCount = await tickets.CountAsync(cancellationToken);
        var offset = (long)(query.Page - 1) * query.PageSize;
        var pageTickets = offset > int.MaxValue
            ? []
            : await ApplyOrdering(tickets, query)
                .Skip((int)offset)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

        return new TicketPage(pageTickets, totalCount);
    }

    public async Task<TicketSummary> GetSummaryAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Tickets
                .GroupBy(_ => 1)
                .Select(tickets => new TicketSummary(
                    tickets.Count(ticket => ticket.Status == TicketStatus.Open),
                    tickets.Count(ticket => ticket.Status == TicketStatus.InProgress),
                    tickets.Count(ticket => ticket.Status == TicketStatus.Resolved),
                    tickets.Count(ticket => ticket.Priority == TicketPriority.High)))
                .SingleOrDefaultAsync(cancellationToken)
            ?? new TicketSummary(0, 0, 0, 0);
    }

    public Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Tickets
            .AsSplitQuery()
            .Include(ticket => ticket.Comments)
            .Include(ticket => ticket.StatusChanges)
            .SingleOrDefaultAsync(ticket => ticket.Id == id, cancellationToken);
    }

    public async Task AddAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        await dbContext.Tickets.AddAsync(ticket, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    // Tickets come from GetByIdAsync on this same scoped context, so change tracking already
    // knows what was modified or added.
    public Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        return SaveTicketChangesAsync(ticket, cancellationToken);
    }

    public Task DeleteAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        dbContext.Tickets.Remove(ticket);
        return SaveTicketChangesAsync(ticket, cancellationToken);
    }

    private async Task SaveTicketChangesAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            // UpdatedAt is a concurrency token: no row matched the value that was loaded.
            throw new TicketConcurrencyException(ticket.Id, exception);
        }
    }

    /// <summary>
    /// Orders by the requested field, then by newest first, then by id so pagination is stable.
    /// </summary>
    private static IOrderedQueryable<Ticket> ApplyOrdering(IQueryable<Ticket> tickets, TicketListQuery query)
    {
        var ordered = query.SortBy switch
        {
            TicketSortField.CreatedAt => OrderBy(tickets, ticket => ticket.CreatedAt, query.SortDescending),
            TicketSortField.Priority => OrderBy(tickets, PriorityRank, query.SortDescending)
                .ThenByDescending(ticket => ticket.CreatedAt),
            TicketSortField.Status => OrderBy(tickets, StatusRank, query.SortDescending)
                .ThenByDescending(ticket => ticket.CreatedAt),
            _ => throw new ArgumentOutOfRangeException(nameof(query), query.SortBy, "Unsupported sort field.")
        };

        return ordered.ThenBy(ticket => ticket.Id);
    }

    private static IOrderedQueryable<Ticket> OrderBy<TKey>(
        IQueryable<Ticket> tickets,
        Expression<Func<Ticket, TKey>> key,
        bool descending)
    {
        return descending ? tickets.OrderByDescending(key) : tickets.OrderBy(key);
    }

    private static string EscapeLikePattern(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
    }
}
