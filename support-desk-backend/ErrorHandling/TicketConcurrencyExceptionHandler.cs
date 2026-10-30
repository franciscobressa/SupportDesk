using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.Api.Domain;

namespace SupportDesk.Api.ErrorHandling;

/// <summary>
/// Maps <see cref="TicketConcurrencyException"/> to <c>409 Conflict</c> in one place, so services and
/// controllers do not need a separate result type for every write that can lose a race.
/// </summary>
public sealed class TicketConcurrencyExceptionHandler(IProblemDetailsService problemDetailsService)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not TicketConcurrencyException)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "This ticket was changed by someone else.",
                Detail = "Reload the ticket to see the latest version, then apply your changes again."
            }
        });
    }
}
