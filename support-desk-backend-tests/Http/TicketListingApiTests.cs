using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.Api.Domain;
using SupportDesk.Api.Tests.TestSupport;

namespace SupportDesk.Api.Tests.Http;

/// <summary>
/// Read-only HTTP contract tests. They share one seeded database for speed, so no test in this
/// class may change data; tests that write live in <see cref="TicketWriteApiTests"/>.
/// </summary>
public sealed class TicketListingApiTests(TicketApiFactory factory) : IClassFixture<TicketApiFactory>
{
    private const int DefaultPageSize = 10;
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetTickets_WithoutParameters_ReturnsFirstPageAndQueueSummary()
    {
        var result = await _client.GetFromJsonAsync<JsonElement>("/api/tickets");

        Assert.Equal(Math.Min(DefaultPageSize, SeedData.Count), result.GetProperty("items").GetArrayLength());
        Assert.Equal(1, result.GetProperty("page").GetInt32());
        Assert.Equal(DefaultPageSize, result.GetProperty("pageSize").GetInt32());
        Assert.Equal(SeedData.Count, result.GetProperty("totalCount").GetInt32());
        Assert.Equal(SeedData.TotalPages(DefaultPageSize), result.GetProperty("totalPages").GetInt32());
        var summary = result.GetProperty("summary");
        Assert.Equal(SeedData.CountWith(TicketStatus.Open), summary.GetProperty("open").GetInt32());
        Assert.Equal(SeedData.CountWith(TicketStatus.InProgress), summary.GetProperty("inProgress").GetInt32());
        Assert.Equal(SeedData.CountWith(TicketStatus.Resolved), summary.GetProperty("resolved").GetInt32());
        Assert.Equal(SeedData.CountWith(TicketPriority.High), summary.GetProperty("highPriority").GetInt32());
    }

    [Fact]
    public async Task GetTickets_ReturnsDistinctPagesAndAnEmptyPageBeyondTheEnd()
    {
        var pastLastPageNumber = SeedData.TotalPages(DefaultPageSize) + 1;
        var firstPage = await _client.GetFromJsonAsync<JsonElement>("/api/tickets?page=1");
        var secondPage = await _client.GetFromJsonAsync<JsonElement>("/api/tickets?page=2");
        var pastLastPage = await _client.GetFromJsonAsync<JsonElement>($"/api/tickets?page={pastLastPageNumber}");

        Assert.Empty(TicketIds(firstPage).Intersect(TicketIds(secondPage)));
        Assert.Equal(2, secondPage.GetProperty("page").GetInt32());
        Assert.Empty(pastLastPage.GetProperty("items").EnumerateArray());
        Assert.Equal(SeedData.Count, pastLastPage.GetProperty("totalCount").GetInt32());
        Assert.Equal(SeedData.TotalPages(DefaultPageSize), pastLastPage.GetProperty("totalPages").GetInt32());
    }

    [Fact]
    public async Task GetTickets_CombinesFiltersWithCaseInsensitiveSearchBeforePaging()
    {
        var expectedCount = SeedData.Tickets.Count(ticket =>
            ticket.Status == TicketStatus.Open
            && ticket.Priority == TicketPriority.High
            && ticket.Title.Contains("reset", StringComparison.OrdinalIgnoreCase));

        var result = await _client.GetFromJsonAsync<JsonElement>(
            "/api/tickets?status=Open&priority=High&search=RESET&pageSize=1");

        Assert.True(expectedCount > 0, "The seed should contain an open, high-priority ticket about a reset.");
        Assert.Equal(expectedCount, result.GetProperty("totalCount").GetInt32());
        var ticket = Assert.Single(result.GetProperty("items").EnumerateArray());
        Assert.Contains("reset", ticket.GetProperty("title").GetString(), StringComparison.OrdinalIgnoreCase);
        // The summary describes the whole queue, not the filtered result.
        Assert.Equal(SeedData.CountWith(TicketStatus.Open), result.GetProperty("summary").GetProperty("open").GetInt32());
    }

    [Fact]
    public async Task GetTickets_CountsFilteredResultsAcrossAllPages()
    {
        var result = await _client.GetFromJsonAsync<JsonElement>("/api/tickets?status=Open&pageSize=1");

        Assert.Equal("Open", Assert.Single(result.GetProperty("items").EnumerateArray()).GetProperty("status").GetString());
        Assert.Equal(SeedData.CountWith(TicketStatus.Open), result.GetProperty("totalCount").GetInt32());
        Assert.Equal(SeedData.CountWith(TicketStatus.Open), result.GetProperty("totalPages").GetInt32());
    }

    [Theory]
    [InlineData("priority", "High,Medium,Low")]
    [InlineData("status", "Closed,Resolved,InProgress,Open")]
    public async Task GetTickets_SortsEnumsByDomainOrderNotAlphabetically(string sortBy, string descendingOrder)
    {
        var rank = descendingOrder.Split(',').Select((value, index) => (value, index)).ToDictionary(x => x.value, x => x.index);

        var result = await _client.GetFromJsonAsync<JsonElement>($"/api/tickets?pageSize=50&sortBy={sortBy}&sortDirection=desc");

        var ranks = result.GetProperty("items").EnumerateArray()
            .Select(ticket => rank[ticket.GetProperty(sortBy).GetString()!])
            .ToArray();
        Assert.Equal(ranks.Order(), ranks);
    }

    [Theory]
    [InlineData("asc", false)]
    [InlineData("desc", true)]
    public async Task GetTickets_SortsByCreatedAtInTheRequestedDirection(string direction, bool descending)
    {
        var result = await _client.GetFromJsonAsync<JsonElement>($"/api/tickets?pageSize=50&sortBy=createdAt&sortDirection={direction}");

        var createdAt = result.GetProperty("items").EnumerateArray()
            .Select(ticket => ticket.GetProperty("createdAt").GetDateTimeOffset())
            .ToArray();
        Assert.Equal(SeedData.Count, createdAt.Length);
        Assert.All(
            createdAt.Zip(createdAt.Skip(1)),
            pair => Assert.True(descending ? pair.First >= pair.Second : pair.First <= pair.Second));
    }

    [Fact]
    public async Task GetTickets_WithNoMatches_ReturnsZeroPages()
    {
        var result = await _client.GetFromJsonAsync<JsonElement>("/api/tickets?search=no-ticket-has-this-title");

        Assert.Empty(result.GetProperty("items").EnumerateArray());
        Assert.Equal(0, result.GetProperty("totalCount").GetInt32());
        Assert.Equal(0, result.GetProperty("totalPages").GetInt32());
    }

    [Theory]
    [InlineData("/api/tickets?status=Unknown")]
    [InlineData("/api/tickets?status=1")]
    [InlineData("/api/tickets?sortBy=unknown")]
    [InlineData("/api/tickets?sortDirection=sideways")]
    [InlineData("/api/tickets?page=0")]
    [InlineData("/api/tickets?pageSize=0")]
    [InlineData("/api/tickets?pageSize=51")]
    public async Task GetTickets_WithInvalidQuery_ReturnsValidationProblem(string path)
    {
        var response = await _client.GetAsync(path);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotEmpty(Assert.IsType<ValidationProblemDetails>(problem).Errors);
    }

    [Fact]
    public async Task GetTicket_WithUnknownId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/tickets/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static IEnumerable<Guid> TicketIds(JsonElement page)
    {
        return page.GetProperty("items").EnumerateArray().Select(ticket => ticket.GetProperty("id").GetGuid());
    }
}
