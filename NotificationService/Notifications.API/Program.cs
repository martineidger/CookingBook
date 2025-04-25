using Messaging.Kafka.Common;
using Messaging.Kafka.Consuming.Abstractions;
using Messaging.Kafka.Consuming;
using Notifications.API.KafkaTests;
using Notifications.API.KafkaTests.Recipes;
using Notifications.API.KafkaTests.Users;
using Notifications.API;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMultiConsumer<string>(builder.Configuration.GetSection("Kafka:Notifications"));

builder.Services.AddScoped<IMessageHandler<NewDailyRecipe>, NewDailyRecipeMessageHandler>();
builder.Services.AddScoped<IMessageHandler<NewSubscription>, NewSubscriptionMessageHandler>();

builder.Services.AddHostedService<NotificationService>();

var app = builder.Build();
app.MapGet("/", () => "Notification service");
app.Run();