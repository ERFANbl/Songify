using Domain.DbMpdels;

namespace Application.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetByUsernameAsync(string username);
    }
} 