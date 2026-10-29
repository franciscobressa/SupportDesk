using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SupportDesk.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedDemoTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Tickets",
                columns: new[] { "Id", "CreatedAt", "CustomerEmail", "CustomerName", "Description", "Priority", "Status", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), new DateTimeOffset(new DateTime(2026, 10, 29, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "alex.rivera@example.com", "Alex Rivera", "The billing page shows the invoice, but the download button returns an error.", "High", "Open", "Cannot download monthly invoice", new DateTimeOffset(new DateTime(2026, 10, 29, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-0000-0000-000000000002"), new DateTimeOffset(new DateTime(2026, 10, 28, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "jamie.patel@example.com", "Jamie Patel", "Please transfer the account owner role to the new operations manager.", "Medium", "InProgress", "Change account owner", new DateTimeOffset(new DateTime(2026, 10, 28, 15, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-0000-0000-000000000003"), new DateTimeOffset(new DateTime(2026, 10, 28, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "morgan.blake@example.com", "Morgan Blake", "The customer is prompted to sign in again every time the app is reopened.", "High", "Open", "Mobile app keeps logging out", new DateTimeOffset(new DateTime(2026, 10, 28, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-0000-0000-000000000004"), new DateTimeOffset(new DateTime(2026, 10, 27, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "sam.carter@example.com", "Sam Carter", "Two identical charges appear for the same subscription renewal.", "High", "Open", "Duplicate charge on latest payment", new DateTimeOffset(new DateTime(2026, 10, 27, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-0000-0000-000000000005"), new DateTimeOffset(new DateTime(2026, 10, 27, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "riley.foster@example.com", "Riley Foster", "The company address was updated in settings but the old address remains on invoices.", "Low", "InProgress", "Update company address", new DateTimeOffset(new DateTime(2026, 10, 27, 15, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-0000-0000-000000000006"), new DateTimeOffset(new DateTime(2026, 10, 26, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "taylor.kim@example.com", "Taylor Kim", "Inviting a new teammate fails with an unexpected error message.", "Medium", "Open", "Unable to invite a teammate", new DateTimeOffset(new DateTime(2026, 10, 26, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-0000-0000-000000000007"), new DateTimeOffset(new DateTime(2026, 10, 26, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "casey.nguyen@example.com", "Casey Nguyen", "The CSV export contains fewer records than the report displayed in the dashboard.", "High", "InProgress", "Export report has missing rows", new DateTimeOffset(new DateTime(2026, 10, 26, 15, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-0000-0000-000000000008"), new DateTimeOffset(new DateTime(2026, 10, 25, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "jordan.ellis@example.com", "Jordan Ellis", "Daily activity notifications are arriving several hours after the events.", "Low", "Open", "Notification emails arrive late", new DateTimeOffset(new DateTime(2026, 10, 25, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-0000-0000-000000000009"), new DateTimeOffset(new DateTime(2026, 10, 25, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "avery.brooks@example.com", "Avery Brooks", "The customer replaced their phone and needs help restoring access to the account.", "High", "Open", "Reset two-factor authentication", new DateTimeOffset(new DateTime(2026, 10, 25, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-0000-0000-000000000010"), new DateTimeOffset(new DateTime(2026, 10, 24, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "drew.parker@example.com", "Drew Parker", "Searching by a known ticket title only returns recently created tickets.", "Medium", "InProgress", "Search does not find older tickets", new DateTimeOffset(new DateTime(2026, 10, 24, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-0000-0000-000000000011"), new DateTimeOffset(new DateTime(2026, 10, 24, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "quinn.murphy@example.com", "Quinn Murphy", "The account still shows the previous plan after the downgrade was confirmed.", "Medium", "Open", "Subscription downgrade not applied", new DateTimeOffset(new DateTime(2026, 10, 24, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-0000-0000-000000000012"), new DateTimeOffset(new DateTime(2026, 10, 23, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "skyler.james@example.com", "Skyler James", "The usage chart has no data for the current month despite recent activity.", "Medium", "Resolved", "Dashboard chart is blank", new DateTimeOffset(new DateTime(2026, 10, 23, 15, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-0000-0000-000000000013"), new DateTimeOffset(new DateTime(2026, 10, 23, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "reese.morgan@example.com", "Reese Morgan", "Activity timestamps appear three hours ahead of the account timezone.", "Low", "Open", "Incorrect timezone in activity log", new DateTimeOffset(new DateTime(2026, 10, 23, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-0000-0000-000000000014"), new DateTimeOffset(new DateTime(2026, 10, 22, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "robin.hayes@example.com", "Robin Hayes", "A valid replacement card is rejected when saved to the billing profile.", "High", "InProgress", "Payment method update rejected", new DateTimeOffset(new DateTime(2026, 10, 22, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-0000-0000-000000000015"), new DateTimeOffset(new DateTime(2026, 10, 22, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "cameron.wells@example.com", "Cameron Wells", "The archive action is disabled for a project that has no active tasks.", "Low", "Open", "Cannot archive completed project", new DateTimeOffset(new DateTime(2026, 10, 22, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-0000-0000-000000000016"), new DateTimeOffset(new DateTime(2026, 10, 21, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "finley.scott@example.com", "Finley Scott", "The invitation link expired before the teammate had a chance to accept it.", "Medium", "Resolved", "Invite link expired too soon", new DateTimeOffset(new DateTime(2026, 10, 21, 15, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-0000-0000-000000000017"), new DateTimeOffset(new DateTime(2026, 10, 21, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "emerson.cole@example.com", "Emerson Cole", "Changing the display name appears successful but reverts after refreshing the page.", "Medium", "Open", "Profile changes are not saved", new DateTimeOffset(new DateTime(2026, 10, 21, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-0000-0000-000000000018"), new DateTimeOffset(new DateTime(2026, 10, 20, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "harper.diaz@example.com", "Harper Diaz", "The integration receives repeated webhook events after returning a successful response.", "High", "InProgress", "Webhook delivery retries unexpectedly", new DateTimeOffset(new DateTime(2026, 10, 20, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-0000-0000-000000000019"), new DateTimeOffset(new DateTime(2026, 10, 20, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "logan.price@example.com", "Logan Price", "Please provide downloadable receipts for the previous three billing periods.", "Low", "Open", "Need a copy of last quarter receipts", new DateTimeOffset(new DateTime(2026, 10, 20, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-0000-0000-000000000020"), new DateTimeOffset(new DateTime(2026, 10, 19, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "peyton.reed@example.com", "Peyton Reed", "Removing a former employee from the workspace returns a permission error.", "Medium", "Open", "Cannot remove former teammate", new DateTimeOffset(new DateTime(2026, 10, 19, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-0000-0000-000000000021"), new DateTimeOffset(new DateTime(2026, 10, 18, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "dakota.lane@example.com", "Dakota Lane", "Selected date and status filters are cleared whenever the report page reloads.", "Low", "Resolved", "Report filters reset after refresh", new DateTimeOffset(new DateTime(2026, 10, 18, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-0000-0000-000000000022"), new DateTimeOffset(new DateTime(2026, 10, 17, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "shiloh.grant@example.com", "Shiloh Grant", "The administrator needs instructions for rotating an API key without downtime.", "Low", "Closed", "API key rotation guidance", new DateTimeOffset(new DateTime(2026, 10, 17, 15, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-0000-0000-000000000023"), new DateTimeOffset(new DateTime(2026, 10, 16, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "frankie.bell@example.com", "Frankie Bell", "PDF attachments show a blank preview although the file can be downloaded.", "Medium", "Open", "Attachment preview does not load", new DateTimeOffset(new DateTime(2026, 10, 16, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-0000-0000-000000000024"), new DateTimeOffset(new DateTime(2026, 10, 15, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "kendall.ross@example.com", "Kendall Ross", "The workspace reached its monthly usage limit earlier than expected.", "High", "InProgress", "Usage limit reached unexpectedly", new DateTimeOffset(new DateTime(2026, 10, 15, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("10000000-0000-0000-0000-000000000025"), new DateTimeOffset(new DateTime(2026, 10, 14, 13, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "sage.turner@example.com", "Sage Turner", "The account owner requested account closure and confirmation of the data retention policy.", "Medium", "Closed", "Close account and confirm data removal", new DateTimeOffset(new DateTime(2026, 10, 14, 15, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                table: "TicketComments",
                columns: new[] { "Id", "Author", "CreatedAt", "Text", "TicketId" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000001"), "Support Manager (mock)", new DateTimeOffset(new DateTime(2026, 10, 29, 14, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "We are reviewing the invoice download issue and will follow up shortly.", new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0000-000000000002"), "Support Manager (mock)", new DateTimeOffset(new DateTime(2026, 10, 28, 14, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "We have started the account ownership transfer review.", new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("20000000-0000-0000-0000-000000000003"), "Support Manager (mock)", new DateTimeOffset(new DateTime(2026, 10, 26, 14, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "We are comparing the exported rows with the report results.", new Guid("10000000-0000-0000-0000-000000000007") },
                    { new Guid("20000000-0000-0000-0000-000000000004"), "Support Manager (mock)", new DateTimeOffset(new DateTime(2026, 10, 23, 14, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "The chart data has been refreshed and should now be visible.", new Guid("10000000-0000-0000-0000-000000000012") },
                    { new Guid("20000000-0000-0000-0000-000000000005"), "Support Manager (mock)", new DateTimeOffset(new DateTime(2026, 10, 21, 14, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "A new invitation link has been sent to the customer.", new Guid("10000000-0000-0000-0000-000000000016") },
                    { new Guid("20000000-0000-0000-0000-000000000006"), "Support Manager (mock)", new DateTimeOffset(new DateTime(2026, 10, 17, 14, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "We shared the key rotation steps with the account administrator.", new Guid("10000000-0000-0000-0000-000000000022") },
                    { new Guid("20000000-0000-0000-0000-000000000007"), "Support Manager (mock)", new DateTimeOffset(new DateTime(2026, 10, 14, 14, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "We confirmed the account closure request and shared the data policy.", new Guid("10000000-0000-0000-0000-000000000025") }
                });

            migrationBuilder.InsertData(
                table: "TicketStatusChanges",
                columns: new[] { "Id", "Author", "CreatedAt", "NewStatus", "PreviousStatus", "TicketId" },
                values: new object[,]
                {
                    { new Guid("30000000-0000-0000-0000-000000000001"), "Support Manager (mock)", new DateTimeOffset(new DateTime(2026, 10, 28, 15, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "InProgress", "Open", new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("30000000-0000-0000-0000-000000000002"), "Support Manager (mock)", new DateTimeOffset(new DateTime(2026, 10, 27, 15, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "InProgress", "Open", new Guid("10000000-0000-0000-0000-000000000005") },
                    { new Guid("30000000-0000-0000-0000-000000000003"), "Support Manager (mock)", new DateTimeOffset(new DateTime(2026, 10, 26, 15, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "InProgress", "Open", new Guid("10000000-0000-0000-0000-000000000007") },
                    { new Guid("30000000-0000-0000-0000-000000000004"), "Support Manager (mock)", new DateTimeOffset(new DateTime(2026, 10, 23, 15, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Resolved", "InProgress", new Guid("10000000-0000-0000-0000-000000000012") },
                    { new Guid("30000000-0000-0000-0000-000000000005"), "Support Manager (mock)", new DateTimeOffset(new DateTime(2026, 10, 21, 15, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Resolved", "InProgress", new Guid("10000000-0000-0000-0000-000000000016") },
                    { new Guid("30000000-0000-0000-0000-000000000006"), "Support Manager (mock)", new DateTimeOffset(new DateTime(2026, 10, 17, 15, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Closed", "Resolved", new Guid("10000000-0000-0000-0000-000000000022") },
                    { new Guid("30000000-0000-0000-0000-000000000007"), "Support Manager (mock)", new DateTimeOffset(new DateTime(2026, 10, 14, 15, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Closed", "Resolved", new Guid("10000000-0000-0000-0000-000000000025") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TicketComments",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "TicketComments",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "TicketComments",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "TicketComments",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "TicketComments",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "TicketComments",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "TicketComments",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "TicketStatusChanges",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "TicketStatusChanges",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "TicketStatusChanges",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "TicketStatusChanges",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "TicketStatusChanges",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "TicketStatusChanges",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "TicketStatusChanges",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000013"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000014"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000015"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000017"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000018"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000019"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000020"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000021"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000023"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000024"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000012"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000016"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000022"));

            migrationBuilder.DeleteData(
                table: "Tickets",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000025"));
        }
    }
}
