namespace Songify.Domain.Interfaces
{
    public interface IRedisService
    {
        Task<string> GetAsync(string key);
        Task SetAsync(string key, string value, TimeSpan? expiry = null);
        Task<bool> RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
    }
} 