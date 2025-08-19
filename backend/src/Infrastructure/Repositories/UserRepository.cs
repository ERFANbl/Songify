using Domain.Consts;
using Domain.DbMpdels;
using EntityFrameworkCore.Configuration;
using Infrastructure.Interfaces;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Songify.Domain.Interfaces;

namespace Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly IRedisService _redisService;
        private readonly SongifyDbContext _context;
        public UserRepository(SongifyDbContext context, IRedisService redisService) : base(context)
        {
            _redisService = redisService;
            _context = context;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Name == username);
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _dbSet
                .Include(u => u.LikedSongs)
                .Include(u => u.Songs)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByTokenAsync(string token) =>
            await _dbSet.FirstOrDefaultAsync(e=>e.Token == token);

        public async Task<int?> GetUserIdByTokenAsync(string token)
        {
            token = TokenHelpers.NormalizeToken(token);
            if (string.IsNullOrEmpty(token)) return null;

            var tokenHash = TokenHelpers.HashToken(token);
            var redisKey = $"{RedisPrefix.RedisTokenToUserKeyPrefix}{tokenHash}";

            var cachedUserId = await _redisService.GetAsync(redisKey);
            if (!string.IsNullOrEmpty(cachedUserId) && int.TryParse(cachedUserId, out var cachedId))
            {
                return cachedId;
            }

            var user = await GetByTokenAsync(token);
            if (user == null) return null;

            var ttl = TokenHelpers.GetTtlFromToken(token);
            if (ttl > TimeSpan.Zero)
            {
                var userTokenKey = $"{RedisPrefix.RedisTokenKeyPrefix}{user.Id}";
                await _redisService.SetAsync(userTokenKey, token, ttl);

                await _redisService.SetAsync(redisKey, user.Id.ToString());
            }
            return user.Id;
        }

        public async Task UpdateWeeklyLogsAsync(int userId, string weeklyLogs)
        {
            var user = await _context.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();

            if (user == null)
                throw new Exception("User Not Found");

            user.FourWeekLogsJson = weeklyLogs;

            await _context.SaveChangesAsync();
        }

    }
} 