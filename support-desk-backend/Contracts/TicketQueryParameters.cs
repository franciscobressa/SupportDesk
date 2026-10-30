using System.ComponentModel.DataAnnotations;
using SupportDesk.Api.Domain;

namespace SupportDesk.Api.Contracts;

/// <summary>
/// Query string of <c>GET /api/tickets</c>. Values are bound as strings so unknown options are
/// reported as validation errors instead of being silently ignored.
/// </summary>
public sealed class TicketQueryParameters : IValidatableObject
{
    public const int MaxPageSize = 50;

    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than or equal to 1.")]
    public int Page { get; set; } = 1;

    [Range(1, MaxPageSize, ErrorMessage = "PageSize must be between 1 and 50.")]
    public int PageSize { get; set; } = 10;

    public string? Status { get; set; }

    public string? Priority { get; set; }

    public string? Search { get; set; }

    public string? SortBy { get; set; }

    public string? SortDirection { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!TryParseOptionalEnum<TicketStatus>(Status, out _))
        {
            yield return new ValidationResult(
                "Status must be Open, InProgress, Resolved, or Closed.",
                [nameof(Status)]);
        }

        if (!TryParseOptionalEnum<TicketPriority>(Priority, out _))
        {
            yield return new ValidationResult("Priority must be Low, Medium, or High.", [nameof(Priority)]);
        }

        if (!TryParseSortField(SortBy, out _))
        {
            yield return new ValidationResult("SortBy must be createdAt, priority, or status.", [nameof(SortBy)]);
        }

        if (!TryParseSortDescending(SortDirection, out _))
        {
            yield return new ValidationResult("SortDirection must be asc or desc.", [nameof(SortDirection)]);
        }
    }

    /// <summary>Converts validated parameters; call only after model validation succeeded.</summary>
    public TicketListQuery ToListQuery()
    {
        TryParseOptionalEnum<TicketStatus>(Status, out var status);
        TryParseOptionalEnum<TicketPriority>(Priority, out var priority);
        TryParseSortField(SortBy, out var sortBy);
        TryParseSortDescending(SortDirection, out var sortDescending);

        return new TicketListQuery(status, priority, Search, sortBy, sortDescending, Page, PageSize);
    }

    private static bool TryParseSortField(string? value, out TicketSortField sortField)
    {
        sortField = TicketSortField.CreatedAt;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        // Enum names match the camelCase API values case-insensitively (createdAt -> CreatedAt).
        if (!TryParseOptionalEnum<TicketSortField>(value, out var parsed) || parsed is null)
        {
            return false;
        }

        sortField = parsed.Value;
        return true;
    }

    private static bool TryParseSortDescending(string? value, out bool sortDescending)
    {
        sortDescending = true;
        if (string.IsNullOrWhiteSpace(value) || string.Equals(value, "desc", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        sortDescending = false;
        return string.Equals(value, "asc", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryParseOptionalEnum<TEnum>(string? value, out TEnum? result)
        where TEnum : struct, Enum
    {
        result = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        // Enum.TryParse alone would also accept numbers such as "1"; only names are part of the contract.
        if (!Enum.GetNames<TEnum>().Contains(value, StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        result = Enum.Parse<TEnum>(value, ignoreCase: true);
        return true;
    }
}
