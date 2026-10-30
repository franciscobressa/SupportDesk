using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportDesk.Api.Data.Migrations
{
    /// <summary>
    /// Changes timestamp columns from DateTimeOffset text to INTEGER UTC ticks so SQLite can sort them.
    /// SQLite rebuilds each table and copies the existing text values unchanged; the data itself is
    /// converted by <see cref="ConvertTimestampTextToUtcTicks"/>, which runs after the rebuild.
    /// </summary>
    public partial class StoreTimestampsAsUtcTicks : Migration
    {
        // Migrations are frozen history, so they keep their own column list instead of sharing one.
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
        }
    }
}
