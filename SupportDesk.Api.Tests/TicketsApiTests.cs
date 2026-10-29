using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace SupportDesk.Api.Tests;

public sealed class TicketsApiTests(TicketApiFactory factory) : IClassFixture<TicketApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Theory]
    [InlineData("/api/tickets?status=Unknown")]
    [InlineData("/api/tickets?sortBy=unknown")]
    [InlineData("/api/tickets?sortDirection=sideways")]
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
