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
using Amazon.Runtime;
using Amazon.S3.Model;
using Amazon.S3;

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

        public async Task<string?> DeleteSongAsync(int userId, int songId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                return null;

            var song = await _songRepository.GetByIdAsync(songId);

            if (song == null)
                return "";

            song.is_deleted = true;

            await _songRepository.UpdateAsync(song);

            await _songRepository.SaveChangesAsync();

            return $"Song deleted succssesfuly";
        }

        public async Task<ICollection<GetSongsMetaDataDTO>?> GetAllSongsMetadataAsync(int userId)
        {
            var songs = await _songRepository.GetUserSongsAsync(userId);

            ICollection<GetSongsMetaDataDTO> result = new List<GetSongsMetaDataDTO>();

            foreach (var song in songs)
            {
                result.Add(new GetSongsMetaDataDTO
                {
                    Id = song.Id,
                    Name = song.Name,
                    Artist = song.Artist,
                    TrackDuration = song.TrackDuration,
                    Lyric = song.Lyric,
                    ForigenKey = song.ForigenKey,
                    Genre = song.Genre,
                    ReleaseDate = song.ReleaseDate,
                });
            }

            return result;
        }

        public async Task<GetSongsMetaDataDTO?> GetSongMetadataByIdAsync(int songId)
        {
            var song = await _songRepository.GetByIdAsync(songId);

            var result = new GetSongsMetaDataDTO{

                Id = song.Id,
                Name = song.Name,
                Artist = song.Artist,
                TrackDuration = song.TrackDuration,
                Lyric = song.Lyric,
                ForigenKey = song.ForigenKey,
                Genre = song.Genre,
                ReleaseDate = song.ReleaseDate,
            };

            return result;
        }

    }
}
