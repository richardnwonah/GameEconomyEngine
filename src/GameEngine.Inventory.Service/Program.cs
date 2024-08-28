using GameEngine.Common.MongoDB;
using GameEngine.Inventory.Service;
using GameEngine.Inventory.Service.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Polly;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

builder.Services.AddMongo(config)
.AddMongoRepository<InventoryItem>("inventoryitems");

builder.Services.AddHttpClient<CatalogClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5253");
})
.AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(
    5,
    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                    + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)),
    onRetry: (outcome, timeSpan, retryAttempt) =>
    {
         var serviceProvider = builder.Services.BuildServiceProvider();  //4:42:17
        serviceProvider.GetService<ILogger<CatalogClient>()?
            .LogWarning($"Delaying for {timespan.TotalSeconds} seconds, then making retry{retryAttempt}");
    }
))
.AddTransientHttpErrorPolicy(builder => builder.Dr<TimeoutRejectedException>().CircuitBreakerAsync(
    3,
    TimeSpan.FromSeconds(15),
    onBreak: (outcome, timespan) =>
    {
       var serviceProvider = ServicesModelBinder.BuildServiceProvider();
        serviceProvider.GetService<ILogger<CatalogClient>()?
            .LogWarning($"Opening the circuit for {timespan.TotalSeconds} seconds...");

    },
    onReset: () => {
        var serviceProvider = ServicesModelBinder.BuildServiceProvider();
        serviceProvider.GetService<ILogger<CatalogClient>()?
            .LogWarning($"Closing the circuit...");

    }
))
.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));

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
