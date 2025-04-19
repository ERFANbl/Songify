using Microsoft.Extensions.Configuration;
using Songify.Domain.Interfaces;
using StackExchange.Redis;

namespace Songify.Infrastructure.Services
{
    public class RedisService : IRedisService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        public RedisService(IConfiguration configuration)
        {
            var config = configuration.GetSection("Redis")["Configuration"];
            _redis = ConnectionMultiplexer.Connect(config);
            _database = _redis.GetDatabase();
        }

        public async Task<string> GetAsync(string key)
        {
            return await _database.StringGetAsync(key);
        }

        public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
        {
            await _database.StringSetAsync(key, value, expiry);
        }

        public async Task<bool> RemoveAsync(string key)
        {
            return await _database.KeyDeleteAsync(key);
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await _database.KeyExistsAsync(key);
        }
    }
} 