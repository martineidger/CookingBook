using Messaging.Kafka.Producing;
using Messaging.Kafka.Producing.Abstractions;
using Microsoft.AspNetCore.Http.HttpResults;
using Recipes.API.GrpcTests;
using Recipes.API.KafkaTests;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<UserInfoService>();
builder.Services.AddGrpcClient<UserService.Protos.UserService.UserServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["UserServiceUrl"]);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    handler.ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    return handler;
});

builder.Services.AddProducer<DailyRecipe>(builder.Configuration.GetSection("Kafka:DailyRecipe"));

var app = builder.Build();

app.MapGet("/get-user", async (UserInfoService userInfoService, string id) =>
{
    var info = await userInfoService.GetUserData(id);
    return Results.Ok(info);
});

app.MapPost("/daily-recipe", (IKafkaProducer<DailyRecipe> producer) => {
    producer.ProduceAsync(new DailyRecipe()
    {
        Name = "Test",
    }, default);
});

app.Run();
