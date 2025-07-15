using Application.Interfaces.Services;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Song;
using Domain.DbMpdels;

namespace Application.Services
{
    public class SongService : ISongService
    {
        private readonly IUserRepository _userRepository;
        private readonly ISongRepository _songRepository;

        public SongService(IUserRepository userRepository, ITokenService tokenService, ISongRepository songRepository)
        {
            _userRepository = userRepository;
            _songRepository = songRepository;
        }

        public async Task<string> UploadSongAsync(byte[] audioData, UploadSongDTO new_song, int userId)
        {
            var audioKey = $"{Guid.NewGuid()}.mp3";
            var song = new Song
            {
                Name = new_song.Name,
                Artist = new_song.Artist,
                TrackDuration = new_song.TrackDuration,
                Lyric = new_song.Lyric,
                MetaData = new_song.MetaData,
                audioForigenKey = audioKey,
                is_deleted = false,
                UserId = userId
            };

            await _songRepository.UploadToS3Async(audioData, audioKey);

            await _songRepository.AddAsync(song);

            await _songRepository.SaveChangesAsync();

            return "Song saved succsessfully";

        }
    }
}
