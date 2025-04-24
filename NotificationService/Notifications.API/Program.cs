
using Messaging.Kafka.Consuming;
using Notifications.API.KafkaTests.Recipes;
using Notifications.API.KafkaTests.Users;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddConsumer<NewSubscription, NewSubscriptionMessageHandler>(
    builder.Configuration.GetSection("Kafka:Users"));

builder.Services.AddConsumer<NewDailyRecipe, NewDailyRecipeMessageHandler>(
    builder.Configuration.GetSection("Kafka:Recipes"));

var app = builder.Build();

app.MapGet("/", () => "Notification service");

var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() =>
{
    Console.WriteLine("Application is stopping. Performing cleanup...");
});

app.Run();