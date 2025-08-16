using Application.Services;
using Domain.DTOs.UserVector;
using Infrastructure.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserVectorController : ControllerBase
    {
        private readonly IUserVectorService _vectorService;

        public UserVectorController(IUserVectorService vectorService)
        {
            _vectorService = vectorService;
        }

        [Route("UpdateUserVector/{userId}")]
        [HttpPut]
        public async Task<IActionResult> UpdateUserVector([FromRoute] int userId, [FromBody] UserInteractionsDTO userInteractions)
        {
            await _vectorService.UpdateUserVectorAsync(userId, userInteractions);

            return Ok();
        }
    }
}
