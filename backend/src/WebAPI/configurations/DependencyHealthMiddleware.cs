using EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace WebAPI.configurations
{
    public static class DependencyHealthMiddlewareExtensions
    {
        public static IApplicationBuilder UseDependencyHealthCheck(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DependencyHealthMiddleware>();
        }
    }

    public class DependencyHealthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly SongifyDbContext _dbContext;
        private readonly IConnectionMultiplexer _redis;
        public DependencyHealthMiddleware(RequestDelegate next, SongifyDbContext dbContext, IConnectionMultiplexer redis)
        {
            _next = next;
            _dbContext = dbContext;
            _redis = redis;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check PostgreSQL
            try
            {
                await _dbContext.Database.ExecuteSqlRawAsync("SELECT 1");
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await context.Response.WriteAsync("PostgreSQL is unavailable: " + ex.Message);
                return;
            }

            // Check Redis
            try
            {
                await _redis.GetDatabase().PingAsync();
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await context.Response.WriteAsync("Redis is unavailable: " + ex.Message);
                return;
            }

            await _next(context);
        }

    }
}
