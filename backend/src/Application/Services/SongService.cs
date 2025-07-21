using Application.Interfaces.Services;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Song;
using Domain.DbMpdels;
using System.Net.Http.Headers;

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

        public async Task<string> UploadToSongEncoderAsync(byte[] audioData, string genre,string releaseDate, string lyric, string Id)
        {
            using var httpClient = new HttpClient();
            using var formContent = new MultipartFormDataContent();

            var fileStreamContent = new StreamContent(new MemoryStream(audioData));
            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("audio/mpeg");

            formContent.Add(fileStreamContent, "audio_file");
            formContent.Add(new StringContent(genre), "genre");
            formContent.Add(new StringContent(releaseDate), "releaseDate");
            formContent.Add(new StringContent(lyric), "lyric");
            formContent.Add(new StringContent(Id), "Id");


            var response = await httpClient.PostAsync("http://python-service/process-audio", formContent);

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> UploadSongAsync(byte[] audioData, UploadSongDTO new_song, int userId)
        {
            var audioKey = $"{Guid.NewGuid()}";
            var song = new Song
            {
                Name = new_song.Name,
                Artist = new_song.Artist,
                TrackDuration = new_song.TrackDuration,
                Lyric = new_song.Lyric,
                Genre = new_song.Genre,
                ReleaseDate = new_song.ReleaseDate,
                ForigenKey = audioKey,
                is_deleted = false,
                UserId = userId
            };

            var response = await UploadToSongEncoderAsync(audioData, song.Genre, song.ReleaseDate, song.Lyric, audioKey);

            // if (response == ...) 

            await _songRepository.UploadToS3Async(audioData, audioKey + ".mp3");

            await _songRepository.AddAsync(song);

            await _songRepository.SaveChangesAsync();

            return "Song saved succsessfully";

        }

        public async Task<string?> DeleteSong(int userId, int songId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                return null;

            var song = await _songRepository.GetByIdAsync(songId);

            if (song == null)
                return "";

            song.is_deleted = true;

            await _songRepository.SaveChangesAsync();

            return $"Song deleted succssesfuly";
        }
    }
}
