namespace SupportDesk.Api.Domain;

/// <summary>Queue-wide counts shown on the dashboard, independent of list filters.</summary>
public sealed record TicketSummary(
    int Open,
    int InProgress,
    int Resolved,
    int HighPriority);
