using Application.Services;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Services;
using Infrastructure.Repositories;
using Infrastructure.Services;
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
            services.AddScoped<ISongRepository, SongRepository>();
            services.AddScoped<IMadeForUserRepository, MadeForUserRepository>();
            services.AddScoped<IStreamRepository, StreamRepository>();
            
            // Add services
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ISongService, SongService>();
            services.AddScoped<IMadeForUserService, MadeForUserService>();
            services.AddScoped<IStreamService, StreamService>();
            services.AddScoped<IUserServices, UserServices>();

            return services;
        }
    }
} 