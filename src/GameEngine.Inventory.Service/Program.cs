using GameEngine.Common.MongoDB;
using GameEngine.Inventory.Service;
using GameEngine.Inventory.Service.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Polly;
using Polly.Extensions.Http;
using Polly.CircuitBreaker;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;
var jitterer = new Random();

builder.Services.AddMongo(config)
.AddMongoRepository<InventoryItem>("inventoryitems");

builder.Services.AddHttpClient<CatalogClient>(client =>
{
   client.BaseAddress = new Uri("http://localhost:5253");
})
.AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.WaitAndRetryAsync(
    5,
    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                    + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)),
    onRetry: (outcome, timeSpan, retryAttempt, context) =>
    {
        var serviceProvider = (IServiceProvider)context["ServiceProvider"];
        var logger = serviceProvider.GetRequiredService<ILogger<CatalogClient>>();
        logger?.LogWarning($"Delaying for {timeSpan.TotalSeconds} seconds, then making retry {retryAttempt}");
    }
))
.AddTransientHttpErrorPolicy(policyBuilder => policyBuilder
    .AdvancedCircuitBreakerAsync(
        failureThreshold: 0.5, // Break on 50% failures
        samplingDuration: TimeSpan.FromSeconds(30), // Over a 30-second window
        minimumThroughput: 7, // Minimum 7 requests in the sampling duration
        durationOfBreak: TimeSpan.FromSeconds(15), // Break for 15 seconds
        onBreak: (outcome, timeSpan, context) =>
        {
            var serviceProvider = (IServiceProvider)context["ServiceProvider"];
            var logger = serviceProvider.GetRequiredService<ILogger<CatalogClient>>();
            logger?.LogWarning($"Opening the circuit for {timeSpan.TotalSeconds} seconds...");
        },
        onReset: (context) =>
        {
            var serviceProvider = (IServiceProvider)context["ServiceProvider"];
            var logger = serviceProvider.GetRequiredService<ILogger<CatalogClient>>();
            logger?.LogWarning("Closing the circuit...");
        }
    )
)
.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(1)));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
