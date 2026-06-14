using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace CloudOps.Api.Infrastructure.Messaging.RabbitMQ
{
    public class RabbitMqPublisher : IMessagePublisher
    {
        private readonly RabbitMqSettings _settings;

        public RabbitMqPublisher(RabbitMqSettings settings)
        {
            _settings = settings;
        }

        public async Task PublishAsync<T>(string queueName, T message)
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password
            };

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queueName,
                body: body
            );
        }
    }
}
