using Application.DTOs.Auth;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.SignupAsync(request);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(request);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return Unauthorized(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // Try to get the user ID from different claim types
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) 
                             ?? User.FindFirst("sub");
                
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return BadRequest("Invalid user ID");
            }

            var result = await _authService.LogoutAsync(userId);
            
            if (result)
            {
                return Ok(new { Message = "Logged out successfully" });
            }
            
            return BadRequest(new { Message = "Failed to logout" });
        }
    }
} 