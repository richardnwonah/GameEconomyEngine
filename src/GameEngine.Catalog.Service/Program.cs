using GameEngine.Catalog.Service.Entities;
using GameEngine.Common.MongoDB;
using GameEngine.Common.Settings;
using Microsoft.AspNetCore.Http.Features;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;


ServiceSettings serviceSettings;


var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddMongo(config)
.AddMongoRepository<Item>("items");
// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.SuppressAsyncSuffixInActionNames = false;
});
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
