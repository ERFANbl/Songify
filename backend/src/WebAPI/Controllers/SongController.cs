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
        [HttpPost]
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

        [Route("Delete/{userId}/{songId}")]
        [HttpPatch]
        public async Task<IActionResult> DeleteSong([FromRoute] int userId, [FromRoute] int songId)
        {
            var result = await _songService.DeleteSongAsync(userId, songId);
            if (result == null)
            {
                return BadRequest("couldn't find user");
            }

            if (result == "")
            {
                return BadRequest("couldn't find song");
            }

            return Ok();
        }

        [Route("GetUploadedSongs/{userId}")]
        [HttpGet]
        public async Task<IActionResult> GetAllUploadedSongsMetadata([FromRoute] int userId)
        {
            return Ok( await _songService.GetAllSongsMetadataAsync(userId) );
        }

        [Route("GetSong/{songId}")]
        [HttpGet]
        public async Task<IActionResult> GetSongMetadata([FromRoute] int songId)
        {
            return Ok(await _songService.GetSongMetadataByIdAsync(songId));
        }

        [Route("LikeSong/{userId}/{songId}")]
        [HttpPut]
        public async Task<IActionResult> LikeSong([FromRoute] int userId, [FromRoute] int songId)
        {
            return Ok(await _songService.LikeSongByIdAsync(userId, songId));
        }
    }
}
