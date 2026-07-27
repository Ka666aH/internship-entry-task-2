using App.Application.Interfaces.Repositories;
using App.Application.Interfaces.Services;
using App.Application.Services;
using App.Infrastructure.Database;
using App.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper));
});

builder.Services.AddScoped<IOperationRepository, OperationRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IOperationService, OperationService>();
builder.Services.AddScoped<IReceiptService, ReceiptService>();

builder.Services.AddHttpClient("Provider", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["PROVIDER_URL"]
        ?? throw new InvalidOperationException("PROVIDER_URL is not set"));
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddHostedService<OperationOutboxWorker>();

builder.Services.AddHealthChecks();
var app = builder.Build();

app.MapControllers();
app.MapHealthChecks("/health");
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider
        .GetRequiredService<AppDbContext>()
        .Database.EnsureCreated();
}

app.Run();
