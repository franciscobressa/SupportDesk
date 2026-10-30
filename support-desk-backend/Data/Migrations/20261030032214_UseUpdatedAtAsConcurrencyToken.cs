using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportDesk.Api.Data.Migrations
{
    /// <summary>
    /// Marks Tickets.UpdatedAt as a concurrency token. This only changes the EF model (EF adds the
    /// column to UPDATE/DELETE WHERE clauses), so there is no schema change to apply.
    /// </summary>
    public partial class UseUpdatedAtAsConcurrencyToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
