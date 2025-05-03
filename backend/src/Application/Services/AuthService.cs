using Application.DTOs.Auth;
using Application.Interfaces;
using Application.Interfaces.Services;
using Domain.DbMpdels;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;

        public AuthService(IUserRepository userRepository, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
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

            // Create new user
            var user = new User
            {
                Name = request.UserName,
                PasswordHash = passwordHash
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