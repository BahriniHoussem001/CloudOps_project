namespace CloudOps.Api.Infrastructure.Messaging
{
    public interface IMessagePublisher
    {
        Task PublishAsync<T>(string queueName, T message);
    }
}
