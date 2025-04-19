using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Songify.Domain.Interfaces;
using Songify.Infrastructure.Services;

namespace Songify.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Redis service
            services.AddSingleton<IRedisService, RedisService>();
            
            // Add other infrastructure services here...
            
            return services;
        }
    }
} 