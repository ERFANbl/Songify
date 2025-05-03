using Application.DTOs.Auth;

namespace Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> SignupAsync(SignupRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<bool> LogoutAsync(int userId);
    }
} 