namespace SupportDesk.Api.Domain;

/// <summary>
/// Field limits shared by request validation and the database schema, so the API never accepts
/// a value the database would reject.
/// </summary>
public static class TicketConstraints
{
    public const int TitleMaxLength = 120;

    public const int DescriptionMaxLength = 5000;

    public const int CustomerNameMaxLength = 120;

    public const int CustomerEmailMaxLength = 254;

    public const int CommentMaxLength = 2000;

    public const int AuthorMaxLength = 120;

    /// <summary>Fixed author for comments and status changes until authentication exists.</summary>
    public const string MockAuthor = "Support Manager (mock)";
}
