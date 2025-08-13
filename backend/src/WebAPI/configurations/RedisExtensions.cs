using Infrastructure.Services;
using Songify.Domain.Interfaces;
using StackExchange.Redis;

namespace WebAPI.configurations
{
    public static class RedisExtensions
    {
        public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var redisConfig = configuration.GetSection("Redis")["Configuration"];
                return ConnectionMultiplexer.Connect($"{redisConfig},abortConnect=false");
            });

            services.AddScoped<IRedisService, RedisService>();

            return services;
        }
    }

}
