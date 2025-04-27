using Messaging.Kafka.Consuming.Abstractions;

namespace Notifications.API.KafkaTests.Recipes
{
    public class NewDailyRecipeMessageHandler(ILogger<NewDailyRecipeMessageHandler> logger) : IMessageHandler<NewDailyRecipe>
    {
        public Task HandleAsync(NewDailyRecipe message, CancellationToken cancellationToken)
        {
            logger.LogInformation($"New daily recipe, id: {message.Id} ,name: {message.Name}");
            return Task.CompletedTask;
        }
    }
}
