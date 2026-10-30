using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportDesk.Api.Data.Migrations
{
    /// <summary>
    /// Converts timestamp values written before <see cref="StoreTimestampsAsUtcTicks"/> from text to
    /// UTC ticks. It is a separate migration because EF runs SQLite table rebuilds at the end of a
    /// migration: here the columns are already INTEGER when the UPDATE runs.
    /// Rows that are already integers are skipped, so the migration is safe to re-run.
    /// </summary>
    public partial class ConvertTimestampTextToUtcTicks : Migration
    {
        private const long UnixEpochTicks = 621355968000000000;

        private static readonly (string Table, string Column)[] TimestampColumns =
        [
            ("Tickets", "CreatedAt"),
            ("Tickets", "UpdatedAt"),
            ("TicketComments", "CreatedAt"),
            ("TicketStatusChanges", "CreatedAt")
        ];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var (table, column) in TimestampColumns)
            {
                // Text format: "yyyy-MM-dd HH:mm:ss[.fffffff]+HH:mm". strftime('%s') applies the offset;
                // the fractional digits are padded to 7 to become ticks.
                migrationBuilder.Sql($"""
                    UPDATE "{table}"
                    SET "{column}" = {UnixEpochTicks}
                        + CAST(strftime('%s', "{column}") AS INTEGER) * 10000000
                        + CASE WHEN substr("{column}", 20, 1) = '.'
                            THEN CAST(substr(substr("{column}", 21, length("{column}") - 26) || '0000000', 1, 7) AS INTEGER)
                            ELSE 0
                          END
                    WHERE typeof("{column}") = 'text';
                    """);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Runs before StoreTimestampsAsUtcTicks.Down turns the columns back into TEXT.
            foreach (var (table, column) in TimestampColumns)
            {
                migrationBuilder.Sql($"""
                    UPDATE "{table}"
                    SET "{column}" = strftime('%Y-%m-%d %H:%M:%S', ("{column}" - {UnixEpochTicks}) / 10000000, 'unixepoch')
                        || '.' || substr('0000000' || (("{column}" - {UnixEpochTicks}) % 10000000), -7)
                        || '+00:00'
                    WHERE typeof("{column}") = 'integer';
                    """);
            }
        }
    }
}
