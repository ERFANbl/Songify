using Application.Interfaces.Services;
using Application.DTOs.Song;
using Application.Services;
using Domain.DbMpdels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SongController : ControllerBase
    {
        private readonly ISongService _songService;

        public SongController(ISongService songService)
        {
            _songService = songService;
        }

        [Route("Upload/{userId}")]
        [HttpPut]
        public async Task<IActionResult> UploadSong([FromBody] byte[] audioData, [FromBody] UploadSongDTO song,[FromRoute] int userId)
        {
            if (audioData == null) {
                return BadRequest("no audio recived");
            }

            if (song == null) {
                return BadRequest("no song data recived");
            }

            return Ok(await _songService.UploadSongAsync(audioData, song, userId));
        }
    }
}
