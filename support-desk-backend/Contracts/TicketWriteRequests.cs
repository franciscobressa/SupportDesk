using System.ComponentModel.DataAnnotations;
using SupportDesk.Api.Domain;

namespace SupportDesk.Api.Contracts;

public abstract class TicketWriteRequest : IValidatableObject
{
    [Required]
    [StringLength(TicketConstraints.TitleMaxLength)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(TicketConstraints.DescriptionMaxLength)]
    public string Description { get; set; } = string.Empty;

    [StringLength(TicketConstraints.CustomerNameMaxLength)]
    public string? CustomerName { get; set; }

    [EmailAddress]
    [StringLength(TicketConstraints.CustomerEmailMaxLength)]
    public string? CustomerEmail { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(CustomerName) && string.IsNullOrWhiteSpace(CustomerEmail))
        {
            yield return new ValidationResult(
                "At least one customer name or email is required.",
                [nameof(CustomerName), nameof(CustomerEmail)]);
        }
    }
}

public sealed class CreateTicketRequest : TicketWriteRequest
{
    [EnumDataType(typeof(TicketStatus))]
    public TicketStatus? Status { get; set; }

    [EnumDataType(typeof(TicketPriority))]
    public TicketPriority? Priority { get; set; }
}

public sealed class UpdateTicketRequest : TicketWriteRequest
{
    [Required]
    [EnumDataType(typeof(TicketStatus))]
    public TicketStatus? Status { get; set; }

    [Required]
    [EnumDataType(typeof(TicketPriority))]
    public TicketPriority? Priority { get; set; }

    /// <summary>The <c>updatedAt</c> value the client loaded; a mismatch means someone else changed the ticket.</summary>
    [Required]
    public DateTimeOffset? ExpectedUpdatedAt { get; set; }
}

public sealed class CreateCommentRequest
{
    [Required]
    [StringLength(TicketConstraints.CommentMaxLength)]
    public string Text { get; set; } = string.Empty;
}
