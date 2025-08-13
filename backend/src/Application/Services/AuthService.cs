using Application.DTOs.Auth;
using Domain.DbMpdels;
using Microsoft.Extensions.Configuration;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Services;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configService;

        public AuthService(IUserRepository userRepository, ITokenService tokenService, IConfiguration configService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _configService = configService;
        }

        public async Task<string> InitialUserVector()
        {
            using var httpClient = new HttpClient();

            var vectorKey = Guid.NewGuid().ToString();

            await httpClient.PostAsync($"http://{_configService["RecommenderServices:UserVectorService:Host"]}:{_configService["RecommenderServices:UserVectorService:Port"]}/InitialUserVector/{vectorKey}", new StringContent(vectorKey));

            return vectorKey;
        }
        public async Task<AuthResponse> SignupAsync(SignupRequest request)
        {
            // Check if user already exists
            var existingUser = await _userRepository.GetByUsernameAsync(request.UserName);
            if (existingUser != null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Username already exists."
                };
            }

            // Hash the password
            string passwordHash = PasswordHasher.HashPassword(request.Password);

            // Initial user vector
            var UserVectorKey = await InitialUserVector();

            // Create new user
            var user = new User
            {
                Name = request.UserName,
                PasswordHash = passwordHash,
                UserVectorId = UserVectorKey,
                DateLimit = DateTime.UtcNow
            };

            // Save the user
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            // Generate token
            string token = _tokenService.GenerateToken(user);
            user.Token = token;
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            // Return success response
            return new AuthResponse
            {
                Success = true,
                Message = "User registered successfully!",
                Token = token,
                UserId = user.Id,
                UserName = user.Name
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            // Find the user by username
            var user = await _userRepository.GetByUsernameAsync(request.UserName);
            if (user == null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid username or password."
                };
            }

            // Verify the password
            if (!PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Invalid username or password."
                };
            }

            // Generate a new token
            string token = _tokenService.GenerateToken(user);

            // Save the token in the database
            user.Token = token;
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            // Return success response
            return new AuthResponse
            {
                Success = true,
                Message = "Login successful!",
                Token = token,
                UserId = user.Id,
                UserName = user.Name
            };
        }

        public async Task<bool> LogoutAsync(int userId)
        {
            // Get the user
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            // Clear the token
            user.Token = null;
            await _userRepository.UpdateAsync(user);
            return await _userRepository.SaveChangesAsync();
        }
    }
} 