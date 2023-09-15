using Microsoft.Extensions.Configuration;
using MongoDB.Bson.Serialization.Serializers;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using GameEngine.Common.Settings;


namespace GameEngine.Common.MongoDB
{
    public static class Extentions
    {
      private static IConfiguration? Configuration;
   
        public static IServiceCollection AddMongo(this IServiceCollection services, IConfiguration configuration)
        {
            Configuration = configuration;
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
            BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));

            services.AddSingleton(serviceProvider =>
            {
                var serviceSettings = Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                var mongoDbSettings = Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
                var mongoCient = new MongoClient(mongoDbSettings.ConnectionString);
                return mongoCient.GetDatabase(serviceSettings.ServiceName);

            });
            return services;
        }

        public static IServiceCollection AddMongoRepository<T>(this IServiceCollection services, string collectionName)
            where T : IEntity
        {
            services.AddSingleton<IRepository<T>>(
                serviceProvider =>
                {
                    var database = serviceProvider.GetService<IMongoDatabase>();
                    return new MongoRepository<T>(database, collectionName);
                });

            return services;
        }

    }
}
