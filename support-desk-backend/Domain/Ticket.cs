namespace SupportDesk.Api.Domain;

/// <summary>
/// Aggregate root for a support ticket. State only changes through its methods, which keep the
/// invariants together: text is stored trimmed, every change moves <see cref="UpdatedAt"/>, and
/// every status change is recorded in <see cref="StatusChanges"/>.
/// </summary>
public sealed class Ticket
{
    private readonly List<TicketComment> _comments = [];
    private readonly List<TicketStatusChange> _statusChanges = [];

    // Used by EF Core when materializing rows.
    private Ticket()
    {
    }

    public Guid Id { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public TicketStatus Status { get; private set; }

    public TicketPriority Priority { get; private set; }

    public string? CustomerName { get; private set; }

    public string? CustomerEmail { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyList<TicketComment> Comments => _comments;

    public IReadOnlyList<TicketStatusChange> StatusChanges => _statusChanges;

    public static Ticket Create(
        string title,
        string description,
        string? customerName,
        string? customerEmail,
        TicketStatus status,
        TicketPriority priority,
        DateTimeOffset now)
    {
        return new Ticket
        {
            // Version 7 GUIDs are time-ordered, which keeps primary key inserts sequential.
            Id = Guid.CreateVersion7(now),
            Title = title.Trim(),
            Description = description.Trim(),
            CustomerName = NormalizeOptional(customerName),
            CustomerEmail = NormalizeOptional(customerEmail),
            Status = status,
            Priority = priority,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    /// <returns><see langword="true"/> when any field changed.</returns>
    public bool UpdateDetails(
        string title,
        string description,
        string? customerName,
        string? customerEmail,
        TicketPriority priority,
        DateTimeOffset now)
    {
        title = title.Trim();
        description = description.Trim();
        customerName = NormalizeOptional(customerName);
        customerEmail = NormalizeOptional(customerEmail);

        if (Title == title
            && Description == description
            && CustomerName == customerName
            && CustomerEmail == customerEmail
            && Priority == priority)
        {
            return false;
        }

        Title = title;
        Description = description;
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        Priority = priority;
        UpdatedAt = now;
        return true;
    }

    /// <returns><see langword="true"/> when the status changed and a history entry was recorded.</returns>
    public bool ChangeStatus(TicketStatus newStatus, string author, DateTimeOffset now)
    {
        if (Status == newStatus)
        {
            return false;
        }

        _statusChanges.Add(TicketStatusChange.Create(Id, Status, newStatus, author, now));
        Status = newStatus;
        UpdatedAt = now;
        return true;
    }

    public TicketComment AddComment(string text, string author, DateTimeOffset now)
    {
        var comment = TicketComment.Create(Id, text.Trim(), author, now);
        _comments.Add(comment);
        UpdatedAt = now;
        return comment;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
