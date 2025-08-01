using Application.Interfaces;
using Domain.DbMpdels;
using EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(SongifyDbContext context) : base(context)
        {
        }

        public async Task<User> GetByUsernameAsync(string username)
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

    }
} 