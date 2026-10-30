using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportDesk.Api.Data.Migrations
{
    /// <summary>
    /// Ids are now created by the domain (Guid v7) and keys are configured as ValueGeneratedNever.
    /// This only changes the EF model; the GUID columns stay as they are.
    /// </summary>
    public partial class GenerateIdsInDomain : Migration
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
