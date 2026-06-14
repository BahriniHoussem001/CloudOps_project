using CloudOps.Api.Common.Responses;
using CloudOps.Api.Infrastructure.Messaging;
using CloudOps.Api.Infrastructure.Messaging.RabbitMQ;
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
builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection("RabbitMQ"));

builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();

    return new RabbitMqSettings
    {
        HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
        Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
        UserName = configuration["RabbitMQ:UserName"] ?? "guest",
        Password = configuration["RabbitMQ:Password"] ?? "guest"
    };
});

builder.Services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();

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