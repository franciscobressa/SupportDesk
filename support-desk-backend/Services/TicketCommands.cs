using SupportDesk.Api.Domain;

namespace SupportDesk.Api.Services;

public sealed record CreateTicketCommand(
    string Title,
    string Description,
    string? CustomerName,
    string? CustomerEmail,
    TicketStatus? Status,
    TicketPriority? Priority);

public sealed record UpdateTicketCommand(
    string Title,
    string Description,
    string? CustomerName,
    string? CustomerEmail,
    TicketStatus Status,
    TicketPriority Priority,
    DateTimeOffset ExpectedUpdatedAt);
