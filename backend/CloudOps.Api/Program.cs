using CloudOps.Api.Common.Middleware;
using CloudOps.Api.Common.Responses;
using CloudOps.Api.Infrastructure.Messaging;
using CloudOps.Api.Infrastructure.Messaging.RabbitMQ;
using CloudOps.Api.Infrastructure.Messaging.RabbitMQ.Consumers;
using CloudOps.Api.Infrastructure.Persistence;
using CloudOps.Api.Modules.Notifications.Endpoints;
using CloudOps.Api.Modules.Requests.Endpoints;
using CloudOps.Api.Modules.Services.Endpoints;
using CloudOps.Api.Modules.Users.Endpoints;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

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
builder.Services.AddHostedService<RequestCreatedConsumer>();
    
var app = builder.Build();
app.UseMiddleware<GlobalExceptionMiddleware>();
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

app.MapGet("/api/health", async (
    AppDbContext dbContext,
    RabbitMqSettings rabbitMqSettings) =>
{
    var databaseStatus = "Disconnected";
    var rabbitMqStatus = "Disconnected";

    try
    {
        var canConnectToDatabase = await dbContext.Database.CanConnectAsync();

        databaseStatus = canConnectToDatabase
            ? "Connected"
            : "Disconnected";
    }
    catch
    {
        databaseStatus = "Disconnected";
    }

    try
    {
        var factory = new ConnectionFactory
        {
            HostName = rabbitMqSettings.HostName,
            Port = rabbitMqSettings.Port,
            UserName = rabbitMqSettings.UserName,
            Password = rabbitMqSettings.Password
        };

        await using var connection = await factory.CreateConnectionAsync();

        rabbitMqStatus = connection.IsOpen
            ? "Connected"
            : "Disconnected";
    }
    catch
    {
        rabbitMqStatus = "Disconnected";
    }

    var globalStatus =
        databaseStatus == "Connected" &&
        rabbitMqStatus == "Connected"
            ? "Healthy"
            : "Unhealthy";

    var healthData = new
    {
        status = globalStatus,
        service = "CloudOps.Api",
        api = "Healthy",
        database = databaseStatus,
        rabbitMq = rabbitMqStatus,
        timestamp = DateTime.UtcNow
    };

    if (globalStatus == "Healthy")
    {
        return Results.Ok(ApiResponse<object>.Ok(
            healthData,
            "Application health checked successfully"
        ));
    }

    return Results.Problem(
        title: "Application is unhealthy",
        detail: "One or more dependencies are not connected.",
        statusCode: StatusCodes.Status503ServiceUnavailable
    );
})
.WithName("HealthCheck")
.WithTags("Health")
.WithOpenApi();


app.MapUserEndpoints();
app.MapServiceEndpoints();
app.MapRequestEndpoints();
app.MapNotificationEndpoints();

app.Run();