using Domain.DbMpdels;

namespace Application.Interfaces.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
} 