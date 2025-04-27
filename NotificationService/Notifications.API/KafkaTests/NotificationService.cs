using Messaging.Kafka.Consuming.Abstractions;
using Messaging.Kafka.Consuming;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Notifications.API.KafkaTests.Recipes;
using Notifications.API.KafkaTests.Users;

namespace Notifications.API
{
    public class NotificationService : BackgroundService
    {
        private readonly MultiTopicKafkaConsumer<string> _consumer;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            MultiTopicKafkaConsumer<string> consumer,
            IServiceProvider serviceProvider,
            ILogger<NotificationService> logger)
        {
            _consumer = consumer;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();

            _consumer.Subscribe<NewDailyRecipe>(
                "daily-recipe",
                scope.ServiceProvider.GetRequiredService<IMessageHandler<NewDailyRecipe>>(),
                "notifications-service-users");

            _consumer.Subscribe<NewSubscription>(
                "new-subscription",
                scope.ServiceProvider.GetRequiredService<IMessageHandler<NewSubscription>>(),
                "notifications-service-recipes");

            _logger.LogInformation("Subscribed to Kafka topics");
            return Task.CompletedTask;
        }
    }
}