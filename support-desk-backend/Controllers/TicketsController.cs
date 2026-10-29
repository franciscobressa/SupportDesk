using Microsoft.AspNetCore.Mvc;
using SupportDesk.Api.Contracts;
using SupportDesk.Api.Domain;
using SupportDesk.Api.Services;

namespace SupportDesk.Api.Controllers;

[ApiController]
[Route("api/tickets")]
public sealed class TicketsController(TicketService ticketService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<TicketListResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<TicketListResponse>> GetAll(
        [FromQuery] TicketQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();
        if (parameters.Page < 1)
        {
            errors[nameof(parameters.Page)] = ["Page must be greater than or equal to 1."];
        }

        if (parameters.PageSize is < 1 or > 50)
        {
            errors[nameof(parameters.PageSize)] = ["PageSize must be between 1 and 50."];
        }

        if (!TryParseOptionalEnum(parameters.Status, out TicketStatus? status))
        {
            errors[nameof(parameters.Status)] = ["Status must be Open, InProgress, Resolved, or Closed."];
        }

        if (!TryParseOptionalEnum(parameters.Priority, out TicketPriority? priority))
        {
            errors[nameof(parameters.Priority)] = ["Priority must be Low, Medium, or High."];
        }

        var sortBy = ParseSortField(parameters.SortBy, errors);
        var sortDescending = ParseSortDirection(parameters.SortDirection, errors);
        if (errors.Count > 0)
        {
            return BadRequest(new ValidationProblemDetails(errors));
        }

        var query = new TicketListQuery(
            status,
            priority,
            parameters.Search,
            sortBy,
            sortDescending,
            parameters.Page,
            parameters.PageSize);
        var result = await ticketService.GetTicketsAsync(query, cancellationToken);
        var items = result.Tickets.Select(ticket => new TicketListItemResponse(
                ticket.Id,
                ticket.Title,
                ticket.Description,
                ticket.Status,
                ticket.Priority,
                ticket.CustomerName,
                ticket.CustomerEmail,
                ticket.CreatedAt,
                ticket.UpdatedAt))
            .ToArray();
        var totalPages = (int)Math.Ceiling(result.TotalCount / (double)parameters.PageSize);

        return Ok(new TicketListResponse(
            items,
            parameters.Page,
            parameters.PageSize,
            result.TotalCount,
            totalPages));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<TicketDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDetailsResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var ticket = await ticketService.GetTicketAsync(id, cancellationToken);
        return ticket is null
            ? NotFound()
            : Ok(TicketDetailsResponse.FromTicket(ticket));
    }

    [HttpPost]
    [ProducesResponseType<TicketDetailsResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TicketDetailsResponse>> Create(
        CreateTicketRequest request,
        CancellationToken cancellationToken)
    {
        var ticket = await ticketService.CreateTicketAsync(
            new CreateTicketCommand(
                request.Title,
                request.Description,
                request.CustomerName,
                request.CustomerEmail,
                request.Status,
                request.Priority),
            cancellationToken);
        var response = TicketDetailsResponse.FromTicket(ticket);

        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<TicketDetailsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketDetailsResponse>> Update(
        Guid id,
        UpdateTicketRequest request,
        CancellationToken cancellationToken)
    {
        var ticket = await ticketService.UpdateTicketAsync(
            id,
            new UpdateTicketCommand(
                request.Title,
                request.Description,
                request.CustomerName,
                request.CustomerEmail,
                request.Status!.Value,
                request.Priority!.Value),
            cancellationToken);

        return ticket is null
            ? NotFound()
            : Ok(TicketDetailsResponse.FromTicket(ticket));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await ticketService.DeleteTicketAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/comments")]
    [ProducesResponseType<TicketCommentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketCommentResponse>> AddComment(
        Guid id,
        CreateCommentRequest request,
        CancellationToken cancellationToken)
    {
        var comment = await ticketService.AddCommentAsync(id, request.Text, cancellationToken);
        if (comment is null)
        {
            return NotFound();
        }

        var response = new TicketCommentResponse(
            comment.Id,
            comment.Text,
            comment.Author,
            comment.CreatedAt);

        return CreatedAtAction(nameof(GetById), new { id }, response);
    }

    private static TicketSortField ParseSortField(
        string? value,
        IDictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(value)
            || string.Equals(value, "createdAt", StringComparison.OrdinalIgnoreCase))
        {
            return TicketSortField.CreatedAt;
        }

        if (string.Equals(value, "priority", StringComparison.OrdinalIgnoreCase))
        {
            return TicketSortField.Priority;
        }

        if (string.Equals(value, "status", StringComparison.OrdinalIgnoreCase))
        {
            return TicketSortField.Status;
        }

        errors[nameof(TicketQueryParameters.SortBy)] = ["SortBy must be createdAt, priority, or status."];
        return TicketSortField.CreatedAt;
    }

    private static bool ParseSortDirection(
        string? value,
        IDictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(value)
            || string.Equals(value, "desc", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(value, "asc", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        errors[nameof(TicketQueryParameters.SortDirection)] = ["SortDirection must be asc or desc."];
        return true;
    }

    private static bool TryParseOptionalEnum<TEnum>(string? value, out TEnum? result)
        where TEnum : struct, Enum
    {
        result = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (!Enum.GetNames<TEnum>().Contains(value, StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        result = Enum.Parse<TEnum>(value, ignoreCase: true);
        return true;
    }
}

public sealed class TicketQueryParameters
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? Status { get; set; }

    public string? Priority { get; set; }

    public string? Search { get; set; }

    public string? SortBy { get; set; }

    public string? SortDirection { get; set; }
}
