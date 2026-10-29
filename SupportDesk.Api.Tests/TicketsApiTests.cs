using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

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
