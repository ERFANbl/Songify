using Domain.DbMpdels;

namespace Infrastructure.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetByUsernameAsync(string username);
        Task<User?> GetUserByIdAsync(int id);
        Task<int?> GetUserIdByTokenAsync(string token);
    }
} 