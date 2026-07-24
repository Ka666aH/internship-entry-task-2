using App.Application.Interfaces.Repositories;
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

var app = builder.Build();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider
        .GetRequiredService<AppDbContext>()
        .Database.EnsureCreated();
}

app.Run();
