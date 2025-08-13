using Application.DTOs.Song;
using Infrastructure.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MadeForUserController : ControllerBase
    {
        private readonly IMadeForUserService _mfuService;

        public MadeForUserController(IMadeForUserService mfuService)
        {
            _mfuService = mfuService;
        }

        [Route("GetAllRecomendedSongs/{userId}")]
        [HttpGet]
        public async Task<ActionResult<ICollection<GetSongsMetaDataDTO>>> GetAllRecomendedSongs(int userId)
        {
            return Ok( await _mfuService.GetWeeklyRecommendedSongsAsync(userId) );
        }
    }
}
