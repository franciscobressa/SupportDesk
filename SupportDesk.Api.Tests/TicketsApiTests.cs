using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SupportDesk.Api.Data;
using SupportDesk.Api.Domain;
using SupportDesk.Api.Repositories;

namespace SupportDesk.Api.Tests;

public sealed class TicketsApiTests(TicketApiFactory factory) : IClassFixture<TicketApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetTicketsAsync_ReturnsTwentyFiveDemoTicketsOnStartup()
    {
        var result = await _client.GetFromJsonAsync<JsonElement>("/api/tickets");

        Assert.Equal(JsonValueKind.Array, result.GetProperty("items").ValueKind);
        Assert.Equal(10, result.GetProperty("items").GetArrayLength());
        Assert.Equal(1, result.GetProperty("page").GetInt32());
        Assert.Equal(10, result.GetProperty("pageSize").GetInt32());
        Assert.Equal(25, result.GetProperty("totalCount").GetInt32());
        Assert.Equal(3, result.GetProperty("totalPages").GetInt32());
        var summary = result.GetProperty("summary");
        Assert.Equal(9, summary.GetProperty("open").GetInt32());
        Assert.Equal(7, summary.GetProperty("inProgress").GetInt32());
        Assert.Equal(5, summary.GetProperty("resolved").GetInt32());
        Assert.Equal(10, summary.GetProperty("highPriority").GetInt32());
    }

    [Fact]
    public async Task DatabaseInitializer_DoesNotRecreateDeletedDemoTicketsOnRestart()
    {
        // Uses its own database because it deletes a demo ticket the other tests count on.
        await using var isolatedFactory = new TicketApiFactory();
        var client = isolatedFactory.CreateClient();
        var firstPage = await client.GetFromJsonAsync<JsonElement>("/api/tickets");
        var deletedTicketId = firstPage.GetProperty("items")[0].GetProperty("id").GetGuid();
        var deleteResponse = await client.DeleteAsync($"/api/tickets/{deletedTicketId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        await DatabaseInitializer.InitializeAsync(isolatedFactory.Services);

        var result = await client.GetFromJsonAsync<JsonElement>("/api/tickets");
        Assert.Equal(24, result.GetProperty("totalCount").GetInt32());
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/tickets/{deletedTicketId}")).StatusCode);
    }

    [Fact]
    public async Task UpdateTicketAsync_ReturnsConflictForStaleVersionAndBadRequestWithoutVersion()
    {
        await using var isolatedFactory = new TicketApiFactory();
        var client = isolatedFactory.CreateClient();
        var ticket = (await client.GetFromJsonAsync<JsonElement>("/api/tickets")).GetProperty("items")[0];
        var ticketId = ticket.GetProperty("id").GetGuid();
        var loadedUpdatedAt = ticket.GetProperty("updatedAt").GetString();
        object UpdateBody(string title, string? expectedUpdatedAt) => new
        {
            title,
            description = ticket.GetProperty("description").GetString(),
            customerName = ticket.GetProperty("customerName").GetString(),
            status = "Closed",
            priority = "Low",
            expectedUpdatedAt
        };

        var firstUpdate = await client.PutAsJsonAsync($"/api/tickets/{ticketId}", UpdateBody("First editor", loadedUpdatedAt));
        var staleUpdate = await client.PutAsJsonAsync($"/api/tickets/{ticketId}", UpdateBody("Second editor", loadedUpdatedAt));
        var missingVersion = await client.PutAsJsonAsync($"/api/tickets/{ticketId}", UpdateBody("No version", null));

        Assert.Equal(HttpStatusCode.OK, firstUpdate.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, staleUpdate.StatusCode);
        Assert.Equal("application/problem+json", staleUpdate.Content.Headers.ContentType?.MediaType);
        Assert.Equal(HttpStatusCode.BadRequest, missingVersion.StatusCode);
        var current = await client.GetFromJsonAsync<JsonElement>($"/api/tickets/{ticketId}");
        Assert.Equal("First editor", current.GetProperty("title").GetString());
    }

    [Fact]
    public async Task TicketRepository_RejectsSaveWhenTicketChangedAfterItWasLoaded()
    {
        await using var isolatedFactory = new TicketApiFactory();
        _ = isolatedFactory.CreateClient();
        await using var firstScope = isolatedFactory.Services.CreateAsyncScope();
        await using var secondScope = isolatedFactory.Services.CreateAsyncScope();
        var firstRepository = firstScope.ServiceProvider.GetRequiredService<ITicketRepository>();
        var secondRepository = secondScope.ServiceProvider.GetRequiredService<ITicketRepository>();
        var ticketId = (await firstRepository.GetPageAsync(
                new TicketListQuery(null, null, null, TicketSortField.CreatedAt, SortDescending: true, PageSize: 1),
                CancellationToken.None))
            .Tickets[0].Id;

        // Both requests load the same version, as two managers opening the ticket at once would.
        var firstCopy = (await firstRepository.GetByIdAsync(ticketId, CancellationToken.None))!;
        var secondCopy = (await secondRepository.GetByIdAsync(ticketId, CancellationToken.None))!;
        Rename(firstCopy, "First save", minutesLater: 1);
        await firstRepository.UpdateAsync(firstCopy, CancellationToken.None);
        Rename(secondCopy, "Second save", minutesLater: 2);

        await Assert.ThrowsAsync<TicketConcurrencyException>(
            () => secondRepository.UpdateAsync(secondCopy, CancellationToken.None));

        static void Rename(Ticket ticket, string title, int minutesLater) => ticket.UpdateDetails(
            title,
            ticket.Description,
            ticket.CustomerName,
            ticket.CustomerEmail,
            ticket.Priority,
            ticket.UpdatedAt.AddMinutes(minutesLater));
    }

    [Fact]
    public async Task CommentAndStatusChange_ArePersistedWithTheTicket()
    {
        // Child entities get their ids from the domain; this proves EF inserts them rather than updating.
        await using var isolatedFactory = new TicketApiFactory();
        var client = isolatedFactory.CreateClient();
        var ticket = (await client.GetFromJsonAsync<JsonElement>("/api/tickets?status=Open")).GetProperty("items")[0];
        var ticketId = ticket.GetProperty("id").GetGuid();

        var commentResponse = await client.PostAsJsonAsync($"/api/tickets/{ticketId}/comments", new { text = "  Checking logs.  " });
        var afterComment = await client.GetFromJsonAsync<JsonElement>($"/api/tickets/{ticketId}");
        var updateResponse = await client.PutAsJsonAsync($"/api/tickets/{ticketId}", new
        {
            title = afterComment.GetProperty("title").GetString(),
            description = afterComment.GetProperty("description").GetString(),
            customerName = afterComment.GetProperty("customerName").GetString(),
            status = "InProgress",
            priority = afterComment.GetProperty("priority").GetString(),
            expectedUpdatedAt = afterComment.GetProperty("updatedAt").GetString()
        });
        var details = await client.GetFromJsonAsync<JsonElement>($"/api/tickets/{ticketId}");

        Assert.Equal(HttpStatusCode.Created, commentResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Contains(
            details.GetProperty("comments").EnumerateArray(),
            comment => comment.GetProperty("text").GetString() == "Checking logs.");
        var change = Assert.Single(details.GetProperty("statusChanges").EnumerateArray());
        Assert.Equal("Open", change.GetProperty("previousStatus").GetString());
        Assert.Equal("InProgress", change.GetProperty("newStatus").GetString());
        Assert.Equal("InProgress", details.GetProperty("status").GetString());
    }

    [Fact]
    public async Task GetTicketsAsync_ReturnsDistinctPagesAndEmptyPageBeyondEnd()
    {
        var firstPage = await _client.GetFromJsonAsync<JsonElement>("/api/tickets?page=1&pageSize=10");
        var secondPage = await _client.GetFromJsonAsync<JsonElement>("/api/tickets?page=2&pageSize=10");
        var pastLastPage = await _client.GetFromJsonAsync<JsonElement>("/api/tickets?page=4&pageSize=10");

        var firstPageIds = firstPage.GetProperty("items").EnumerateArray()
            .Select(ticket => ticket.GetProperty("id").GetGuid());
        var secondPageIds = secondPage.GetProperty("items").EnumerateArray()
            .Select(ticket => ticket.GetProperty("id").GetGuid());

        Assert.Empty(firstPageIds.Intersect(secondPageIds));
        Assert.Equal(2, secondPage.GetProperty("page").GetInt32());
        Assert.Equal(10, secondPage.GetProperty("items").GetArrayLength());
        Assert.Empty(pastLastPage.GetProperty("items").EnumerateArray());
        Assert.Equal(25, pastLastPage.GetProperty("totalCount").GetInt32());
        Assert.Equal(3, pastLastPage.GetProperty("totalPages").GetInt32());
    }

    [Fact]
    public async Task GetTicketsAsync_AppliesFiltersAndCaseInsensitiveSearchBeforePagination()
    {
        var result = await _client.GetFromJsonAsync<JsonElement>(
            "/api/tickets?status=Open&priority=High&search=RESET&pageSize=1");

        var items = result.GetProperty("items");
        Assert.Single(items.EnumerateArray());
        Assert.Equal(1, result.GetProperty("totalCount").GetInt32());
        Assert.Equal("Unable to reset account password", items[0].GetProperty("title").GetString());
        Assert.Equal(9, result.GetProperty("summary").GetProperty("open").GetInt32());
    }

    [Fact]
    public async Task GetTicketsAsync_AppliesStatusFilterBeforeCountingAndPagination()
    {
        var result = await _client.GetFromJsonAsync<JsonElement>(
            "/api/tickets?status=Open&pageSize=1");

        Assert.Single(result.GetProperty("items").EnumerateArray());
        Assert.Equal("Open", result.GetProperty("items")[0].GetProperty("status").GetString());
        Assert.True(result.GetProperty("totalCount").GetInt32() > 1);
        Assert.Equal(result.GetProperty("totalCount").GetInt32(), result.GetProperty("totalPages").GetInt32());
    }

    [Fact]
    public async Task GetTicketsAsync_SortsPrioritiesByDomainOrder()
    {
        var result = await _client.GetFromJsonAsync<JsonElement>(
            "/api/tickets?pageSize=25&sortBy=priority&sortDirection=desc");

        var priorities = result.GetProperty("items").EnumerateArray()
            .Select(ticket => ticket.GetProperty("priority").GetString())
            .ToArray();
        var ranks = priorities.Select(priority => priority switch
        {
            "High" => 3,
            "Medium" => 2,
            "Low" => 1,
            _ => 0
        });

        Assert.Equal(ranks.OrderDescending(), ranks);
    }

    [Fact]
    public async Task GetTicketsAsync_SortsStatusesByDomainOrder()
    {
        var result = await _client.GetFromJsonAsync<JsonElement>(
            "/api/tickets?pageSize=25&sortBy=status&sortDirection=desc");

        var statuses = result.GetProperty("items").EnumerateArray()
            .Select(ticket => ticket.GetProperty("status").GetString())
            .ToArray();
        var ranks = statuses.Select(status => status switch
        {
            "Closed" => 4,
            "Resolved" => 3,
            "InProgress" => 2,
            "Open" => 1,
            _ => 0
        });

        Assert.Equal(ranks.OrderDescending(), ranks);
    }

    [Fact]
    public async Task GetTicketsAsync_ReturnsZeroPagesWhenFiltersHaveNoMatches()
    {
        var result = await _client.GetFromJsonAsync<JsonElement>(
            "/api/tickets?search=no-ticket-has-this-title");

        Assert.Empty(result.GetProperty("items").EnumerateArray());
        Assert.Equal(0, result.GetProperty("totalCount").GetInt32());
        Assert.Equal(0, result.GetProperty("totalPages").GetInt32());
    }

    [Theory]
    [InlineData("asc", false)]
    [InlineData("desc", true)]
    public async Task GetTicketsAsync_SortsByCreatedAtInRequestedDirection(
        string direction,
        bool descending)
    {
        var result = await _client.GetFromJsonAsync<JsonElement>(
            $"/api/tickets?pageSize=25&sortBy=createdAt&sortDirection={direction}");

        var createdAtDates = result.GetProperty("items").EnumerateArray()
            .Select(ticket => ticket.GetProperty("createdAt").GetDateTimeOffset())
            .ToArray();

        Assert.Equal(25, createdAtDates.Length);
        Assert.All(
            createdAtDates.Zip(createdAtDates.Skip(1)),
            pair => Assert.True(descending ? pair.First >= pair.Second : pair.First <= pair.Second));
    }

    [Theory]
    [InlineData("/api/tickets?status=Unknown")]
    [InlineData("/api/tickets?sortBy=unknown")]
    [InlineData("/api/tickets?sortDirection=sideways")]
    [InlineData("/api/tickets?page=0")]
    [InlineData("/api/tickets?pageSize=0")]
    [InlineData("/api/tickets?pageSize=51")]
    public async Task GetTicketsAsync_ReturnsValidationProblemForInvalidQuery(string path)
    {
        var response = await _client.GetAsync(path);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.NotEmpty(problem.Errors);
    }

    [Fact]
    public async Task CreateTicketAsync_ReturnsValidationProblemWhenCustomerIdentityIsMissing()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/tickets",
            new
            {
                title = "Ticket without customer",
                description = "The request has no customer name or email.",
                customerName = "",
                customerEmail = ""
            });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains(problem.Errors.Keys, key =>
            key.Contains("CustomerName", StringComparison.OrdinalIgnoreCase)
            || key.Contains("CustomerEmail", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetTicketAsync_ReturnsNotFoundForUnknownId()
    {
        var response = await _client.GetAsync($"/api/tickets/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateAndDeleteTicketAsync_ReturnsCreatedLocationAndNoContent()
    {
        var createResponse = await _client.PostAsJsonAsync(
            "/api/tickets",
            new
            {
                title = "Created by integration test",
                description = "This ticket is stored in the isolated test database.",
                customerName = "Test Customer",
                status = "Open",
                priority = "Medium"
            });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);
        Assert.Contains("/api/tickets/", createResponse.Headers.Location!.ToString(), StringComparison.Ordinal);

        using var ticketJson = await JsonDocument.ParseAsync(await createResponse.Content.ReadAsStreamAsync());
        var ticketId = ticketJson.RootElement.GetProperty("id").GetGuid();
        var deleteResponse = await _client.DeleteAsync($"/api/tickets/{ticketId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/tickets/{ticketId}")).StatusCode);
    }
}
