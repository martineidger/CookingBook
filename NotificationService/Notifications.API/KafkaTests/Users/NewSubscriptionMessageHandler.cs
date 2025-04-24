using Messaging.Kafka.Consuming.Abstractions;

namespace Notifications.API.KafkaTests.Users
{
    public class NewSubscriptionMessageHandler(ILogger<NewSubscriptionMessageHandler> logger) : IMessageHandler<NewSubscription>
    {
        public Task HandleAsync(NewSubscription message, CancellationToken cancellationToken)
        {
            logger.LogInformation($"New subscription, id: {message.Id}");
            return Task.CompletedTask;
        }
    }
}
