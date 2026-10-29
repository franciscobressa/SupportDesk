using Microsoft.EntityFrameworkCore;
using SupportDesk.Api.Data;
using SupportDesk.Api.Domain;

namespace SupportDesk.Api.Repositories;

public sealed class EfCoreTicketRepository(SupportDeskDbContext dbContext) : ITicketRepository
{
    public async Task<IReadOnlyList<Ticket>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Tickets
            .AsNoTracking()
            .ToListAsync(cancellationToken);
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

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Ticket ticket, CancellationToken cancellationToken)
    {
        dbContext.Tickets.Remove(ticket);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
