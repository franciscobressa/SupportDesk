using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportDesk.Api.Data.Migrations
{
    /// <summary>
    /// Stores timestamps as UTC ticks (INTEGER) instead of DateTimeOffset text so SQLite can sort them.
    /// SQLite rebuilds the tables and copies the old text values unchanged, so existing rows are
    /// converted explicitly after the column changes.
    /// </summary>
    public partial class StoreTimestampsAsUtcTicks : Migration
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
                migrationBuilder.AlterColumn<long>(
                    name: column,
                    table: table,
                    type: "INTEGER",
                    nullable: false,
                    oldClrType: typeof(DateTimeOffset),
                    oldType: "TEXT");
            }

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
            foreach (var (table, column) in TimestampColumns)
            {
                migrationBuilder.AlterColumn<DateTimeOffset>(
                    name: column,
                    table: table,
                    type: "TEXT",
                    nullable: false,
                    oldClrType: typeof(long),
                    oldType: "INTEGER");
            }

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
