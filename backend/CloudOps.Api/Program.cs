using CloudOps.Api.Common.Responses;
using CloudOps.Api.Infrastructure.Persistence;
using CloudOps.Api.Modules.Notifications.Endpoints;
using CloudOps.Api.Modules.Requests.Endpoints;
using CloudOps.Api.Modules.Services.Endpoints;
using CloudOps.Api.Modules.Users.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/health", () =>
{
    var healthData = new
    {
        status = "Healthy",
        service = "CloudOps.Api",
        timestamp = DateTime.UtcNow
    };

    return Results.Ok(ApiResponse<object>.Ok(
        healthData,
        "CloudOps API is running successfully"
    ));
})
.WithName("HealthCheck")
.WithOpenApi();
app.MapUserEndpoints();
app.MapServiceEndpoints();
app.MapRequestEndpoints();
app.MapNotificationEndpoints();

app.Run();