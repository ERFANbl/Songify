using Application.DTOs.Auth;
using Domain.DbMpdels;

namespace Infrastructure.Interfaces.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
} 