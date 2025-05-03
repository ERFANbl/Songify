using Application.Interfaces;
using Application.Interfaces.Services;
using Application.Services;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Songify.Domain.Interfaces;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Redis service
            services.AddSingleton<IRedisService, RedisService>();
            
            // Add repositories
            services.AddScoped<IUserRepository, UserRepository>();
            
            // Add services
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuthService, AuthService>();
            
            return services;
        }
    }
} 