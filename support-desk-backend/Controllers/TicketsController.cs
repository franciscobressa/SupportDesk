using Microsoft.AspNetCore.Mvc;
using SupportDesk.Api.Contracts;
using SupportDesk.Api.Services;

namespace SupportDesk.Api.Controllers;

[ApiController]
[Route("api/tickets")]
public sealed class TicketsController(TicketService ticketService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<TicketListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TicketListResponse>> GetAll(
        [FromQuery] TicketQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        // [ApiController] has already returned 400 if the parameters failed validation.
        var result = await ticketService.GetTicketsAsync(parameters.ToListQuery(), cancellationToken);
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
        var summary = new TicketSummaryResponse(
            result.Summary.Open,
            result.Summary.InProgress,
            result.Summary.Resolved,
            result.Summary.HighPriority);

        return Ok(new TicketListResponse(
            items,
            parameters.Page,
            parameters.PageSize,
            result.TotalCount,
            totalPages,
            summary));
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
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
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
                request.Priority!.Value,
                request.ExpectedUpdatedAt!.Value),
            cancellationToken);

        return ticket is null
            ? NotFound()
            : Ok(TicketDetailsResponse.FromTicket(ticket));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await ticketService.DeleteTicketAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/comments")]
    [ProducesResponseType<TicketCommentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
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
}
