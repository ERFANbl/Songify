﻿using Application.DTOs.Song;
using Infrastructure.Interfaces.Services;
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
        public async Task<IActionResult> UploadSong([FromForm] UploadSongDTO song, [FromRoute] int userId)
        {
            if (song.audioData == null)
            {
                return BadRequest("no audio recived");
            }

            if (song == null)
            {
                return BadRequest("no song data recived");
            }

            return Ok(await _songService.UploadSongAsync(song, userId));
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
            return Ok(await _songService.GetAllSongsMetadataAsync(userId));
        }

        [Route("GetSong/{userId}/{songId}")]
        [HttpGet]
        public async Task<IActionResult> GetSongMetadata([FromRoute] int userId, [FromRoute] int songId)
        {
            return Ok(await _songService.GetSongMetadataByIdAsync(songId, userId));
        }

        
        [Route("LikeSong/{userId}/{songId}")]
        [HttpPut]
        public async Task<IActionResult> LikeSong([FromRoute] int userId, [FromRoute] int songId)
        {
            await _songService.LikeSong(userId, songId);
            return Accepted();
        }
    }
}
