using Microsoft.EntityFrameworkCore;
using SupportDesk.Api.Data;
using SupportDesk.Api.Domain;

namespace SupportDesk.Api.Repositories;

public sealed class EfCoreTicketRepository(SupportDeskDbContext dbContext) : ITicketRepository
{
    public async Task<TicketPage> GetPageAsync(TicketListQuery query, CancellationToken cancellationToken)
    {
        var summary = await dbContext.Tickets
            .GroupBy(_ => 1)
            .Select(tickets => new TicketSummary(
                tickets.Count(ticket => ticket.Status == TicketStatus.Open),
                tickets.Count(ticket => ticket.Status == TicketStatus.InProgress),
                tickets.Count(ticket => ticket.Status == TicketStatus.Resolved),
                tickets.Count(ticket => ticket.Priority == TicketPriority.High)))
            .SingleOrDefaultAsync(cancellationToken)
            ?? new TicketSummary(0, 0, 0, 0);

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
        var orderedTickets = ApplyOrdering(tickets, query);
        var offset = (long)(query.Page - 1) * query.PageSize;
        var pageTickets = offset > int.MaxValue
            ? []
            : await orderedTickets
                .Skip((int)offset)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

        return new TicketPage(pageTickets, totalCount, summary);
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

    public async Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        if (dbContext.Entry(ticket).State == EntityState.Detached)
        {
            dbContext.Tickets.Update(ticket);
        }

        await SaveTicketChangesAsync(ticket, cancellationToken);
    }

    public async Task DeleteAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        dbContext.Tickets.Remove(ticket);
        await SaveTicketChangesAsync(ticket, cancellationToken);
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

    private static IOrderedQueryable<Ticket> ApplyOrdering(
        IQueryable<Ticket> tickets,
        TicketListQuery query)
    {
        return query.SortBy switch
        {
            TicketSortField.CreatedAt when query.SortDescending =>
                tickets.OrderByDescending(ticket => ticket.CreatedAt).ThenBy(ticket => ticket.Id),
            TicketSortField.CreatedAt =>
                tickets.OrderBy(ticket => ticket.CreatedAt).ThenBy(ticket => ticket.Id),
            TicketSortField.Priority when query.SortDescending =>
                tickets.OrderByDescending(ticket =>
                        ticket.Priority == TicketPriority.High ? 3
                        : ticket.Priority == TicketPriority.Medium ? 2
                        : 1)
                    .ThenByDescending(ticket => ticket.CreatedAt)
                    .ThenBy(ticket => ticket.Id),
            TicketSortField.Priority =>
                tickets.OrderBy(ticket =>
                        ticket.Priority == TicketPriority.High ? 3
                        : ticket.Priority == TicketPriority.Medium ? 2
                        : 1)
                    .ThenByDescending(ticket => ticket.CreatedAt)
                    .ThenBy(ticket => ticket.Id),
            TicketSortField.Status when query.SortDescending =>
                tickets.OrderByDescending(ticket =>
                        ticket.Status == TicketStatus.Closed ? 4
                        : ticket.Status == TicketStatus.Resolved ? 3
                        : ticket.Status == TicketStatus.InProgress ? 2
                        : 1)
                    .ThenByDescending(ticket => ticket.CreatedAt)
                    .ThenBy(ticket => ticket.Id),
            TicketSortField.Status =>
                tickets.OrderBy(ticket =>
                        ticket.Status == TicketStatus.Closed ? 4
                        : ticket.Status == TicketStatus.Resolved ? 3
                        : ticket.Status == TicketStatus.InProgress ? 2
                        : 1)
                    .ThenByDescending(ticket => ticket.CreatedAt)
                    .ThenBy(ticket => ticket.Id),
            _ => throw new ArgumentOutOfRangeException(nameof(query.SortBy))
        };
    }

    private static string EscapeLikePattern(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
    }
}
