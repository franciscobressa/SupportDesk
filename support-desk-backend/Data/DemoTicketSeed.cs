using SupportDesk.Api.Domain;

namespace SupportDesk.Api.Data;

public static class DemoTicketSeed
{
    private static readonly DateTimeOffset ReferenceTime =
        new(2026, 10, 29, 13, 0, 0, TimeSpan.Zero);

    public static Ticket[] Tickets =>
    [
        CreateTicket(1, "Cannot download monthly invoice", "The billing page shows the invoice, but the download button returns an error.", TicketStatus.Open, TicketPriority.High, "Alex Rivera", "alex.rivera@example.com", 0),
        CreateTicket(2, "Change account owner", "Please transfer the account owner role to the new operations manager.", TicketStatus.InProgress, TicketPriority.Medium, "Jamie Patel", "jamie.patel@example.com", 1),
        CreateTicket(3, "Mobile app keeps logging out", "The customer is prompted to sign in again every time the app is reopened.", TicketStatus.Open, TicketPriority.High, "Morgan Blake", "morgan.blake@example.com", 1),
        CreateTicket(4, "Duplicate charge on latest payment", "Two identical charges appear for the same subscription renewal.", TicketStatus.Open, TicketPriority.High, "Sam Carter", "sam.carter@example.com", 2),
        CreateTicket(5, "Update company address", "The company address was updated in settings but the old address remains on invoices.", TicketStatus.InProgress, TicketPriority.Low, "Riley Foster", "riley.foster@example.com", 2),
        CreateTicket(6, "Unable to invite a teammate", "Inviting a new teammate fails with an unexpected error message.", TicketStatus.Open, TicketPriority.Medium, "Taylor Kim", "taylor.kim@example.com", 3),
        CreateTicket(7, "Export report has missing rows", "The CSV export contains fewer records than the report displayed in the dashboard.", TicketStatus.InProgress, TicketPriority.High, "Casey Nguyen", "casey.nguyen@example.com", 3),
        CreateTicket(8, "Notification emails arrive late", "Daily activity notifications are arriving several hours after the events.", TicketStatus.Open, TicketPriority.Low, "Jordan Ellis", "jordan.ellis@example.com", 4),
        CreateTicket(9, "Reset two-factor authentication", "The customer replaced their phone and needs help restoring access to the account.", TicketStatus.Open, TicketPriority.High, "Avery Brooks", "avery.brooks@example.com", 4),
        CreateTicket(10, "Search does not find older tickets", "Searching by a known ticket title only returns recently created tickets.", TicketStatus.InProgress, TicketPriority.Medium, "Drew Parker", "drew.parker@example.com", 5),
        CreateTicket(11, "Subscription downgrade not applied", "The account still shows the previous plan after the downgrade was confirmed.", TicketStatus.Open, TicketPriority.Medium, "Quinn Murphy", "quinn.murphy@example.com", 5),
        CreateTicket(12, "Dashboard chart is blank", "The usage chart has no data for the current month despite recent activity.", TicketStatus.Resolved, TicketPriority.Medium, "Skyler James", "skyler.james@example.com", 6),
        CreateTicket(13, "Incorrect timezone in activity log", "Activity timestamps appear three hours ahead of the account timezone.", TicketStatus.Open, TicketPriority.Low, "Reese Morgan", "reese.morgan@example.com", 6),
        CreateTicket(14, "Payment method update rejected", "A valid replacement card is rejected when saved to the billing profile.", TicketStatus.InProgress, TicketPriority.High, "Robin Hayes", "robin.hayes@example.com", 7),
        CreateTicket(15, "Cannot archive completed project", "The archive action is disabled for a project that has no active tasks.", TicketStatus.Open, TicketPriority.Low, "Cameron Wells", "cameron.wells@example.com", 7),
        CreateTicket(16, "Invite link expired too soon", "The invitation link expired before the teammate had a chance to accept it.", TicketStatus.Resolved, TicketPriority.Medium, "Finley Scott", "finley.scott@example.com", 8),
        CreateTicket(17, "Profile changes are not saved", "Changing the display name appears successful but reverts after refreshing the page.", TicketStatus.Open, TicketPriority.Medium, "Emerson Cole", "emerson.cole@example.com", 8),
        CreateTicket(18, "Webhook delivery retries unexpectedly", "The integration receives repeated webhook events after returning a successful response.", TicketStatus.InProgress, TicketPriority.High, "Harper Diaz", "harper.diaz@example.com", 9),
        CreateTicket(19, "Need a copy of last quarter receipts", "Please provide downloadable receipts for the previous three billing periods.", TicketStatus.Open, TicketPriority.Low, "Logan Price", "logan.price@example.com", 9),
        CreateTicket(20, "Cannot remove former teammate", "Removing a former employee from the workspace returns a permission error.", TicketStatus.Open, TicketPriority.Medium, "Peyton Reed", "peyton.reed@example.com", 10),
        CreateTicket(21, "Report filters reset after refresh", "Selected date and status filters are cleared whenever the report page reloads.", TicketStatus.Resolved, TicketPriority.Low, "Dakota Lane", "dakota.lane@example.com", 11),
        CreateTicket(22, "API key rotation guidance", "The administrator needs instructions for rotating an API key without downtime.", TicketStatus.Closed, TicketPriority.Low, "Shiloh Grant", "shiloh.grant@example.com", 12),
        CreateTicket(23, "Attachment preview does not load", "PDF attachments show a blank preview although the file can be downloaded.", TicketStatus.Open, TicketPriority.Medium, "Frankie Bell", "frankie.bell@example.com", 13),
        CreateTicket(24, "Usage limit reached unexpectedly", "The workspace reached its monthly usage limit earlier than expected.", TicketStatus.InProgress, TicketPriority.High, "Kendall Ross", "kendall.ross@example.com", 14),
        CreateTicket(25, "Close account and confirm data removal", "The account owner requested account closure and confirmation of the data retention policy.", TicketStatus.Closed, TicketPriority.Medium, "Sage Turner", "sage.turner@example.com", 15)
    ];

    public static TicketComment[] Comments =>
    [
        CreateComment(1, 1, "We are reviewing the invoice download issue and will follow up shortly.", 0),
        CreateComment(2, 2, "We have started the account ownership transfer review.", 1),
        CreateComment(3, 7, "We are comparing the exported rows with the report results.", 3),
        CreateComment(4, 12, "The chart data has been refreshed and should now be visible.", 6),
        CreateComment(5, 16, "A new invitation link has been sent to the customer.", 8),
        CreateComment(6, 22, "We shared the key rotation steps with the account administrator.", 12),
        CreateComment(7, 25, "We confirmed the account closure request and shared the data policy.", 15)
    ];

    public static TicketStatusChange[] StatusChanges =>
    [
        CreateStatusChange(1, 2, TicketStatus.Open, TicketStatus.InProgress, 1),
        CreateStatusChange(2, 5, TicketStatus.Open, TicketStatus.InProgress, 2),
        CreateStatusChange(3, 7, TicketStatus.Open, TicketStatus.InProgress, 3),
        CreateStatusChange(4, 12, TicketStatus.InProgress, TicketStatus.Resolved, 6),
        CreateStatusChange(5, 16, TicketStatus.InProgress, TicketStatus.Resolved, 8),
        CreateStatusChange(6, 22, TicketStatus.Resolved, TicketStatus.Closed, 12),
        CreateStatusChange(7, 25, TicketStatus.Resolved, TicketStatus.Closed, 15)
    ];

    private static Ticket CreateTicket(
        int id,
        string title,
        string description,
        TicketStatus status,
        TicketPriority priority,
        string customerName,
        string customerEmail,
        int daysAgo)
    {
        var createdAt = ReferenceTime.AddDays(-daysAgo);
        var updatedAt = StatusChanges
            .FirstOrDefault(change => change.TicketId == TicketId(id))?.CreatedAt ?? createdAt;

        return new Ticket
        {
            Id = TicketId(id),
            Title = title,
            Description = description,
            Status = status,
            Priority = priority,
            CustomerName = customerName,
            CustomerEmail = customerEmail,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }

    private static TicketComment CreateComment(int id, int ticketId, string text, int daysAgo)
    {
        return new TicketComment
        {
            Id = EntityId("20000000", id),
            TicketId = TicketId(ticketId),
            Text = text,
            Author = "Support Manager (mock)",
            CreatedAt = ReferenceTime.AddDays(-daysAgo).AddHours(1)
        };
    }

    private static TicketStatusChange CreateStatusChange(
        int id,
        int ticketId,
        TicketStatus previousStatus,
        TicketStatus newStatus,
        int daysAgo)
    {
        return new TicketStatusChange
        {
            Id = EntityId("30000000", id),
            TicketId = TicketId(ticketId),
            PreviousStatus = previousStatus,
            NewStatus = newStatus,
            Author = "Support Manager (mock)",
            CreatedAt = ReferenceTime.AddDays(-daysAgo).AddHours(2)
        };
    }

    private static Guid TicketId(int id) => EntityId("10000000", id);

    private static Guid EntityId(string prefix, int id) =>
        Guid.Parse($"{prefix}-0000-0000-0000-{id:D12}");
}
