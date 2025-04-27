using Messaging.Kafka.Producing;
using Messaging.Kafka.Producing.Abstractions;
using Users.API.GrpcTests;
using Users.API.Tests;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

builder.Services.AddGrpc();
builder.Services.AddSingleton<UserForRecipeService>();


builder.Services.AddProducer<Subscription>(builder.Configuration.GetSection("Kafka:NewSubscription"));

var app = builder.Build();


app.MapGrpcService<UserForRecipeService>();
app.MapGet("/", () => "UserService gRPC endpoint");

app.MapPost("/subscribe", (IKafkaProducer<Subscription> producer) => {
    producer.ProduceAsync(new Subscription(), default);
});

app.Run();
