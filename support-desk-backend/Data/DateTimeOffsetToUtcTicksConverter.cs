using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SupportDesk.Api.Data;

// SQLite has no native DateTimeOffset type: EF Core stores it as TEXT and cannot translate
// ORDER BY or range comparisons on it. Persisting UTC ticks keeps those operations in SQL.
public sealed class DateTimeOffsetToUtcTicksConverter() : ValueConverter<DateTimeOffset, long>(
    value => value.UtcTicks,
    value => new DateTimeOffset(value, TimeSpan.Zero));
