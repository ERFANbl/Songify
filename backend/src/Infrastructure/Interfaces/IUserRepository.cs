using Domain.DbMpdels;

namespace Infrastructure.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        public Task<User> GetByUsernameAsync(string username);
        public Task<User?> GetUserByIdAsync(int id);
        public Task<int?> GetUserIdByTokenAsync(string token);
        public Task UpdateWeeklyLogsAsync(int userId, string weeklyLogs);
    }
} 