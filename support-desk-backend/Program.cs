using System.Text.Json.Serialization;
using SupportDesk.Api.Data;
using SupportDesk.Api.ErrorHandling;
using SupportDesk.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Container platforms (Vercel) assign the port through PORT; locally the launch profile decides.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(namingPolicy: null, allowIntegerValues: false)));

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<TicketConcurrencyExceptionHandler>();
builder.Services.AddSupportDeskPersistence(builder.Configuration, builder.Environment);
builder.Services.AddScoped<TicketService>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddCors(options =>
{
    // Open for the public demo only: the API has no authentication or private data.
    // Restrict to the frontend origins before adding either.
    options.AddPolicy("Frontend", policy =>
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("Frontend");
app.UseAuthorization();
app.MapControllers();

// Fine for a single-instance demo; production would run migrations as a deployment step.
await DatabaseInitializer.InitializeAsync(app.Services);

app.Run();

// Makes the entry point visible to WebApplicationFactory<Program> in the integration tests.
public partial class Program
{
}
