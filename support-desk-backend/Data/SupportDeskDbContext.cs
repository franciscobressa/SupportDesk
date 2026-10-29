using Microsoft.EntityFrameworkCore;
using SupportDesk.Api.Domain;

namespace SupportDesk.Api.Data;

public sealed class SupportDeskDbContext(DbContextOptions<SupportDeskDbContext> options) : DbContext(options)
{
    public DbSet<Ticket> Tickets => Set<Ticket>();

    public DbSet<TicketComment> TicketComments => Set<TicketComment>();

    public DbSet<TicketStatusChange> TicketStatusChanges => Set<TicketStatusChange>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(ticket => ticket.Id);
            entity.Property(ticket => ticket.Title).HasMaxLength(120).IsRequired();
            entity.Property(ticket => ticket.Description).HasMaxLength(5000).IsRequired();
            entity.Property(ticket => ticket.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(ticket => ticket.Priority).HasConversion<string>().HasMaxLength(16);
            entity.Property(ticket => ticket.CustomerName).HasMaxLength(120);
            entity.Property(ticket => ticket.CustomerEmail).HasMaxLength(254);
            entity.HasIndex(ticket => ticket.CreatedAt);
            entity.HasMany(ticket => ticket.Comments)
                .WithOne(comment => comment.Ticket)
                .HasForeignKey(comment => comment.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(ticket => ticket.StatusChanges)
                .WithOne(change => change.Ticket)
                .HasForeignKey(change => change.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TicketComment>(entity =>
        {
            entity.HasKey(comment => comment.Id);
            entity.Property(comment => comment.Text).HasMaxLength(2000).IsRequired();
            entity.Property(comment => comment.Author).HasMaxLength(120).IsRequired();
        });

        modelBuilder.Entity<TicketStatusChange>(entity =>
        {
            entity.HasKey(change => change.Id);
            entity.Property(change => change.PreviousStatus).HasConversion<string>().HasMaxLength(32);
            entity.Property(change => change.NewStatus).HasConversion<string>().HasMaxLength(32);
            entity.Property(change => change.Author).HasMaxLength(120).IsRequired();
        });
    }
}
