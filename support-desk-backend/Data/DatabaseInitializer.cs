using Microsoft.EntityFrameworkCore;
using SupportDesk.Api.Domain;

namespace SupportDesk.Api.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SupportDeskDbContext>();

        await dbContext.Database.MigrateAsync(cancellationToken);

        if (await dbContext.Tickets.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        dbContext.Tickets.AddRange(
            CreateTicket(
                "Unable to reset account password",
                "The password reset link expires before the customer can complete the process.",
                TicketStatus.Open,
                TicketPriority.High,
                "Avery Morgan",
                "avery.morgan@example.com",
                now.AddHours(-2)),
            CreateTicket(
                "Invoice details do not match",
                "The latest invoice shows an outdated billing address.",
                TicketStatus.InProgress,
                TicketPriority.Medium,
                "Jordan Lee",
                "jordan.lee@example.com",
                now.AddDays(-1),
                TicketStatus.Open),
            CreateTicket(
                "Export customer data",
                "The customer needs a CSV export of their account activity.",
                TicketStatus.Open,
                TicketPriority.Low,
                "Taylor Reed",
                "taylor.reed@example.com",
                now.AddDays(-2)),
            CreateTicket(
                "Dashboard loads slowly",
                "The dashboard takes more than ten seconds to load during peak hours.",
                TicketStatus.Resolved,
                TicketPriority.High,
                "Morgan Ellis",
                "morgan.ellis@example.com",
                now.AddDays(-3),
                TicketStatus.InProgress),
            CreateTicket(
                "Update notification preferences",
                "The customer would like to disable weekly summary emails.",
                TicketStatus.Closed,
                TicketPriority.Low,
                "Casey Brooks",
                "casey.brooks@example.com",
                now.AddDays(-5),
                TicketStatus.Resolved),
            CreateTicket(
                "Two-factor authentication setup",
                "The setup code is not accepted by the authenticator app.",
                TicketStatus.InProgress,
                TicketPriority.High,
                "Riley Chen",
                "riley.chen@example.com",
                now.AddDays(-6),
                TicketStatus.Open));

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Ticket CreateTicket(
        string title,
        string description,
        TicketStatus status,
        TicketPriority priority,
        string customerName,
        string customerEmail,
        DateTimeOffset createdAt,
        TicketStatus? previousStatus = null)
    {
        var ticket = new Ticket
        {
            Title = title,
            Description = description,
            Status = status,
            Priority = priority,
            CustomerName = customerName,
            CustomerEmail = customerEmail,
            CreatedAt = createdAt,
            UpdatedAt = previousStatus is null ? createdAt : createdAt.AddHours(2)
        };

        if (previousStatus is not null)
        {
            ticket.StatusChanges.Add(new TicketStatusChange
            {
                PreviousStatus = previousStatus.Value,
                NewStatus = status,
                Author = "Support Manager (mock)",
                CreatedAt = createdAt.AddHours(2)
            });

            ticket.Comments.Add(new TicketComment
            {
                Text = "We are reviewing this request and will follow up shortly.",
                Author = "Support Manager (mock)",
                CreatedAt = createdAt.AddHours(1)
            });
        }

        return ticket;
    }
}
