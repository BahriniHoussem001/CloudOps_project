using CloudOps.Api.Infrastructure.Persistence;
using CloudOps.Api.Modules.Notifications.Models;
using CloudOps.Api.Modules.Requests.Events;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace CloudOps.Api.Infrastructure.Messaging.RabbitMQ.Consumers
{
    public class RequestCreatedConsumer : BackgroundService
    {
        private readonly RabbitMqSettings _settings;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<RequestCreatedConsumer> _logger;

        public RequestCreatedConsumer(
            RabbitMqSettings settings,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<RequestCreatedConsumer> logger)
        {
            _settings = settings;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            await using var connection = await factory.CreateConnectionAsync(stoppingToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await channel.QueueDeclareAsync(
                queue: "request-created-queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken
            );

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (_, eventArgs) =>
            {
                try
                {
                    var body = eventArgs.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);

                    var requestCreatedEvent = JsonSerializer.Deserialize<RequestCreatedEvent>(json);

                    if (requestCreatedEvent is null)
                    {
                        _logger.LogWarning("Received invalid RequestCreatedEvent message");

                        await channel.BasicAckAsync(
                            deliveryTag: eventArgs.DeliveryTag,
                            multiple: false
                        );

                        return;
                    }

                    using var scope = _serviceScopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var notificationAlreadyExists = await dbContext.Notifications
                     .AnyAsync(notification =>
            notification.RelatedRequestId == requestCreatedEvent.RequestId
    );

                    if (notificationAlreadyExists)
                    {
                        _logger.LogInformation(
                            "Notification already exists for request {RequestId}. Message skipped.",
                            requestCreatedEvent.RequestId
                        );

                        await channel.BasicAckAsync(
                            deliveryTag: eventArgs.DeliveryTag,
                            multiple: false
                        );

                        return;
                    }

                    var notification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = requestCreatedEvent.ClientId,
                        RelatedRequestId = requestCreatedEvent.RequestId,
                        Title = "New service request created",
                        Message = $"Your request '{requestCreatedEvent.Title}' has been created successfully.",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    dbContext.Notifications.Add(notification);
                    await dbContext.SaveChangesAsync();

                    await channel.BasicAckAsync(
                        deliveryTag: eventArgs.DeliveryTag,
                        multiple: false
                    );

                    _logger.LogInformation(
                        "Notification created for request {RequestId}",
                        requestCreatedEvent.RequestId
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while processing RequestCreatedEvent");

                    await channel.BasicNackAsync(
                        deliveryTag: eventArgs.DeliveryTag,
                        multiple: false,
                        requeue: true
                    );
                }
            };

            await channel.BasicConsumeAsync(
                queue: "request-created-queue",
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken
            );

            _logger.LogInformation("RequestCreatedConsumer started and listening to request-created-queue");

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
