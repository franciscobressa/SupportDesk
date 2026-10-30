using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.Api.Tests.TestSupport;

namespace SupportDesk.Api.Tests.Http;

/// <summary>
/// HTTP contract tests for create, update, delete and comments. xUnit creates a new instance per
/// test, so every test gets its own freshly seeded database and cannot affect the others.
/// </summary>
public sealed class TicketWriteApiTests : IAsyncLifetime
{
    private readonly TicketApiFactory _factory = new();
    private readonly HttpClient _client;

    public TicketWriteApiTests()
    {
        _client = _factory.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _factory.DisposeAsync().AsTask();

    [Fact]
    public async Task CreateThenDelete_ReturnsCreatedWithLocationThenNoContent()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/tickets", new
        {
            title = "Created by integration test",
            description = "Stored in the test's own database.",
            customerName = "Test Customer"
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var ticketId = created.GetProperty("id").GetGuid();
        Assert.EndsWith($"/api/tickets/{ticketId}", createResponse.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Equal("Open", created.GetProperty("status").GetString());
        Assert.Equal("Medium", created.GetProperty("priority").GetString());

        var deleteResponse = await _client.DeleteAsync($"/api/tickets/{ticketId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/tickets/{ticketId}")).StatusCode);
    }

    [Fact]
    public async Task CreateTicket_WithoutCustomerNameOrEmail_ReturnsValidationProblem()
    {
        var response = await _client.PostAsJsonAsync("/api/tickets", new
        {
            title = "Ticket without customer",
            description = "The request has no customer name or email.",
            customerName = "",
            customerEmail = ""
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = Assert.IsType<ValidationProblemDetails>(await response.Content.ReadFromJsonAsync<ValidationProblemDetails>());
        Assert.Contains(problem.Errors.Keys, key =>
            key.Contains("CustomerName", StringComparison.OrdinalIgnoreCase)
            || key.Contains("CustomerEmail", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task UpdateTicket_WithStaleVersion_ReturnsConflictAndKeepsTheFirstEdit()
    {
        var ticket = await FirstTicketAsync();
        var loadedVersion = ticket.GetProperty("updatedAt").GetString();
        var ticketId = ticket.GetProperty("id").GetGuid();

        var firstEdit = await _client.PutAsJsonAsync($"/api/tickets/{ticketId}", UpdateBody(ticket, title: "First editor", expectedUpdatedAt: loadedVersion));
        var staleEdit = await _client.PutAsJsonAsync($"/api/tickets/{ticketId}", UpdateBody(ticket, title: "Second editor", expectedUpdatedAt: loadedVersion));
        var unversionedEdit = await _client.PutAsJsonAsync($"/api/tickets/{ticketId}", UpdateBody(ticket, title: "No version", expectedUpdatedAt: null));

        Assert.Equal(HttpStatusCode.OK, firstEdit.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, staleEdit.StatusCode);
        Assert.Equal("application/problem+json", staleEdit.Content.Headers.ContentType?.MediaType);
        Assert.Equal(HttpStatusCode.BadRequest, unversionedEdit.StatusCode);
        var current = await _client.GetFromJsonAsync<JsonElement>($"/api/tickets/{ticketId}");
        Assert.Equal("First editor", current.GetProperty("title").GetString());
    }

    [Fact]
    public async Task CommentAndStatusChange_ArePersistedWithTheTicket()
    {
        // Child entities get their ids from the domain; this proves EF inserts them rather than updating.
        var ticket = await FirstTicketAsync("?status=Open");
        var ticketId = ticket.GetProperty("id").GetGuid();

        var commentResponse = await _client.PostAsJsonAsync($"/api/tickets/{ticketId}/comments", new { text = "  Checking logs.  " });
        var afterComment = await _client.GetFromJsonAsync<JsonElement>($"/api/tickets/{ticketId}");
        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/tickets/{ticketId}",
            UpdateBody(afterComment, status: "InProgress", expectedUpdatedAt: afterComment.GetProperty("updatedAt").GetString()));
        var details = await _client.GetFromJsonAsync<JsonElement>($"/api/tickets/{ticketId}");

        Assert.Equal(HttpStatusCode.Created, commentResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Contains(details.GetProperty("comments").EnumerateArray(), comment => comment.GetProperty("text").GetString() == "Checking logs.");
        var change = Assert.Single(details.GetProperty("statusChanges").EnumerateArray());
        Assert.Equal("Open", change.GetProperty("previousStatus").GetString());
        Assert.Equal("InProgress", change.GetProperty("newStatus").GetString());
    }

    [Fact]
    public async Task WriteEndpoints_WithUnknownTicket_ReturnNotFound()
    {
        var unknownId = Guid.NewGuid();
        var validUpdate = new
        {
            title = "Title",
            description = "Description",
            customerName = "Customer",
            status = "Open",
            priority = "Low",
            expectedUpdatedAt = DateTimeOffset.UtcNow
        };

        var update = await _client.PutAsJsonAsync($"/api/tickets/{unknownId}", validUpdate);
        var delete = await _client.DeleteAsync($"/api/tickets/{unknownId}");
        var comment = await _client.PostAsJsonAsync($"/api/tickets/{unknownId}/comments", new { text = "Hello" });

        Assert.Equal(HttpStatusCode.NotFound, update.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, comment.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AddComment_WithBlankText_ReturnsValidationProblemAndStoresNothing(string text)
    {
        var ticket = await FirstTicketAsync();
        var ticketId = ticket.GetProperty("id").GetGuid();
        var commentsBefore = (await _client.GetFromJsonAsync<JsonElement>($"/api/tickets/{ticketId}")).GetProperty("comments").GetArrayLength();

        var response = await _client.PostAsJsonAsync($"/api/tickets/{ticketId}/comments", new { text });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var commentsAfter = (await _client.GetFromJsonAsync<JsonElement>($"/api/tickets/{ticketId}")).GetProperty("comments").GetArrayLength();
        Assert.Equal(commentsBefore, commentsAfter);
    }

    private async Task<JsonElement> FirstTicketAsync(string query = "")
    {
        var page = await _client.GetFromJsonAsync<JsonElement>($"/api/tickets{query}");
        return page.GetProperty("items")[0];
    }

    // Repeats the ticket's current values; each test overrides only what it exercises.
    private static object UpdateBody(JsonElement ticket, string? expectedUpdatedAt, string? title = null, string? status = null)
    {
        return new
        {
            title = title ?? ticket.GetProperty("title").GetString(),
            description = ticket.GetProperty("description").GetString(),
            customerName = ticket.GetProperty("customerName").GetString(),
            customerEmail = ticket.GetProperty("customerEmail").GetString(),
            status = status ?? ticket.GetProperty("status").GetString(),
            priority = ticket.GetProperty("priority").GetString(),
            expectedUpdatedAt
        };
    }
}
