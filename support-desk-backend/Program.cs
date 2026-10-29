using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SupportDesk.Api.Data;
using SupportDesk.Api.Repositories;
using SupportDesk.Api.Services;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddDbContext<SupportDeskDbContext>((serviceProvider, options) =>
{
    var connectionString = serviceProvider
        .GetRequiredService<IConfiguration>()
        .GetConnectionString("SupportDesk")
        ?? throw new InvalidOperationException("The SupportDesk connection string is not configured.");
    options.UseNpgsql(connectionString);
});
builder.Services.AddScoped<ITicketRepository, EfCoreTicketRepository>();
builder.Services.AddScoped<TicketService>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddCors(options =>
{
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

app.Run();

public partial class Program
{
}
