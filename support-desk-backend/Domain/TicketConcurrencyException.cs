namespace SupportDesk.Api.Domain;

/// <summary>
/// Raised when a ticket changed after the caller loaded it, so applying the write would
/// silently overwrite someone else's update.
/// </summary>
public sealed class TicketConcurrencyException(Guid ticketId, Exception? innerException = null)
    : Exception($"Ticket {ticketId} was modified by another request.", innerException)
{
    public Guid TicketId { get; } = ticketId;
}
