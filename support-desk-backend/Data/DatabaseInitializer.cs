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

        // Seed only an empty database so tickets deleted by users are not recreated on restart.
        if (await dbContext.Tickets.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = scope.ServiceProvider.GetRequiredService<TimeProvider>().GetUtcNow();
        var demoTickets = new[]
        {
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
                TicketStatus.Open),
            CreateTicket(
                "Unable to download monthly invoice",
                "The invoice download returns an empty PDF for the latest billing cycle.",
                TicketStatus.Open,
                TicketPriority.High,
                "Jamie Foster",
                "jamie.foster@example.com",
                now.AddDays(-7)),
            CreateTicket(
                "Change account owner",
                "The account owner left the company and the administrator needs to transfer ownership.",
                TicketStatus.InProgress,
                TicketPriority.Medium,
                "Noah Bennett",
                "noah.bennett@example.com",
                now.AddDays(-8),
                TicketStatus.Open),
            CreateTicket(
                "Mobile app keeps signing out",
                "The customer is prompted to sign in again several times a day on the mobile app.",
                TicketStatus.Open,
                TicketPriority.Medium,
                "Emma Collins",
                "emma.collins@example.com",
                now.AddDays(-9)),
            CreateTicket(
                "Incorrect tax amount on checkout",
                "The tax estimate differs from the final amount shown after placing an order.",
                TicketStatus.Resolved,
                TicketPriority.High,
                "Liam Parker",
                "liam.parker@example.com",
                now.AddDays(-10),
                TicketStatus.InProgress),
            CreateTicket(
                "Add a second billing contact",
                "The customer wants invoices copied to a finance team mailbox.",
                TicketStatus.Closed,
                TicketPriority.Low,
                "Olivia Murphy",
                "olivia.murphy@example.com",
                now.AddDays(-11),
                TicketStatus.Resolved),
            CreateTicket(
                "Webhook delivery is delayed",
                "Order update webhooks arrive several minutes after the event occurs.",
                TicketStatus.InProgress,
                TicketPriority.High,
                "Ethan Rivera",
                "ethan.rivera@example.com",
                now.AddDays(-12),
                TicketStatus.Open),
            CreateTicket(
                "Cannot upload profile image",
                "Uploading a square PNG profile image fails with a generic error message.",
                TicketStatus.Open,
                TicketPriority.Low,
                "Ava Cooper",
                "ava.cooper@example.com",
                now.AddDays(-13)),
            CreateTicket(
                "Duplicate contacts in customer list",
                "Several contacts appear twice after importing the latest spreadsheet.",
                TicketStatus.Resolved,
                TicketPriority.Medium,
                "Lucas Mitchell",
                "lucas.mitchell@example.com",
                now.AddDays(-14),
                TicketStatus.InProgress),
            CreateTicket(
                "Request for data retention details",
                "The customer needs documentation explaining how long account records are retained.",
                TicketStatus.Closed,
                TicketPriority.Low,
                "Sophia Turner",
                "sophia.turner@example.com",
                now.AddDays(-15),
                TicketStatus.Resolved),
            CreateTicket(
                "Team members cannot access shared inbox",
                "Newly invited team members can sign in but do not see the shared support inbox.",
                TicketStatus.Open,
                TicketPriority.High,
                "Mason Phillips",
                "mason.phillips@example.com",
                now.AddDays(-16)),
            CreateTicket(
                "Search ignores hyphens in order IDs",
                "Searching for an order ID containing hyphens returns no matches.",
                TicketStatus.InProgress,
                TicketPriority.Medium,
                "Isabella Parker",
                "isabella.parker@example.com",
                now.AddDays(-17),
                TicketStatus.Open),
            CreateTicket(
                "Subscription renewal date looks wrong",
                "The account page shows a renewal date one day earlier than the confirmation email.",
                TicketStatus.Resolved,
                TicketPriority.Medium,
                "Logan Evans",
                "logan.evans@example.com",
                now.AddDays(-18),
                TicketStatus.InProgress),
            CreateTicket(
                "Export is missing archived records",
                "The CSV export does not include records that were archived earlier this year.",
                TicketStatus.Open,
                TicketPriority.Medium,
                "Mia Edwards",
                "mia.edwards@example.com",
                now.AddDays(-19)),
            CreateTicket(
                "Unable to remove former team member",
                "Removing a former employee from the team settings page results in a server error.",
                TicketStatus.Closed,
                TicketPriority.High,
                "Elijah Stewart",
                "elijah.stewart@example.com",
                now.AddDays(-20),
                TicketStatus.Resolved),
            CreateTicket(
                "Email notifications arrive twice",
                "The customer receives two identical notifications for each ticket update.",
                TicketStatus.InProgress,
                TicketPriority.Low,
                "Charlotte Morris",
                "charlotte.morris@example.com",
                now.AddDays(-21),
                TicketStatus.Open),
            CreateTicket(
                "Product catalog changes not reflected",
                "Recently updated product descriptions are still showing the previous text.",
                TicketStatus.Open,
                TicketPriority.Low,
                "James Rogers",
                "james.rogers@example.com",
                now.AddDays(-22)),
            CreateTicket(
                "Two-factor recovery codes unavailable",
                "The administrator cannot find the recovery codes after enabling two-factor authentication.",
                TicketStatus.Resolved,
                TicketPriority.High,
                "Amelia Parker",
                "amelia.parker@example.com",
                now.AddDays(-23),
                TicketStatus.InProgress),
            CreateTicket(
                "Request to update company name",
                "The legal company name changed and should be updated on invoices and account documents.",
                TicketStatus.Open,
                TicketPriority.Medium,
                "Benjamin Bailey",
                "benjamin.bailey@example.com",
                now.AddDays(-24)),
            CreateTicket(
                "SLA notification sent to the wrong contact",
                "The escalation email was sent to a former account contact instead of the current administrator.",
                TicketStatus.InProgress,
                TicketPriority.High,
                "Natalie Hughes",
                "natalie.hughes@example.com",
                now.AddDays(-25),
                TicketStatus.Open)
        };

        dbContext.Tickets.AddRange(demoTickets);
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
