using Application.DTOs.Song;
using Application.Interfaces;
using Application.Interfaces.Services;
using Domain.DbMpdels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;

namespace Application.Services
{
    public class SongService : ISongService
    {
        private readonly IUserRepository _userRepository;
        private readonly ISongRepository _songRepository;
        private readonly IConfiguration _configService;

        public SongService(IUserRepository userRepository, ITokenService tokenService, ISongRepository songRepository, IConfiguration configService)
        {
            _userRepository = userRepository;
            _songRepository = songRepository;
            _configService = configService;
        }

        private async Task<string> UploadToSongEncoderAsync(IFormFile audioFile, string genre, string releaseDate, string lyric, string audioKey)
        {
            using var client = new HttpClient();
            using var content = new MultipartFormDataContent();

            // Add the file
            var fileStream = audioFile.OpenReadStream();
            var fileStreamContent = new StreamContent(fileStream);
            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(audioFile.ContentType);
            content.Add(fileStreamContent, "audio_file", audioFile.FileName);

            // Add the other form fields
            content.Add(new StringContent(genre ?? ""), "genre");
            content.Add(new StringContent(releaseDate ?? ""), "releaseDate");
            content.Add(new StringContent(lyric ?? ""), "lyric");
            content.Add(new StringContent(audioKey), "Id");

            // Send to Python service
            var response = await client.PostAsync($"http://{_configService["RecommenderServices:EncoderService:Host"]}:{_configService["RecommenderServices:EncoderService:Port"]}/EncodeSong", content);
            return await response.Content.ReadAsStringAsync();
        }


        public async Task<string> UploadSongAsync(UploadSongDTO newSong, int userId)
        {
            var audioKey = $"{Guid.NewGuid()}";

            var song = new Song
            {
                Name = newSong.Name,
                Artist = newSong.Artist,
                TrackDuration = newSong.TrackDuration,
                Lyric = newSong.Lyric,
                Genre = newSong.Genre,
                ReleaseDate = newSong.ReleaseDate,
                ForigenKey = audioKey,
                is_deleted = false,
                UserId = userId
            };

            var response = await UploadToSongEncoderAsync(newSong.audioData, song.Genre, song.ReleaseDate, song.Lyric, audioKey);

            using (var s3Stream = newSong.audioData.OpenReadStream())
            {
                await _songRepository.UploadToS3Async(s3Stream, audioKey + ".mp3");
            }

            await _songRepository.AddAsync(song);
            await _songRepository.SaveChangesAsync();

            return "Song saved successfully";
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

            return songs;
        }

        // TODO: Fill Is_Liked for DTO in this methode

        public async Task<GetSongsMetaDataDTO?> GetSongMetadataByIdAsync(int songId, int userId)
        {
            var song = await _songRepository.GetSongMetadataByIdAsync(songId, userId);

            return song;
        }

        public async Task LikeSong(int userId , int songId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            var song = await _songRepository.GetByIdAsync(songId);
            if (song == null)
                throw new Exception("Song not found");

            if (song.LikedByUsers == null)
                song.LikedByUsers = new List<UserLikedSongs>();

            if (!song.LikedByUsers.Any(u => u.Id == userId))
            {
                song.LikedByUsers.Add(new UserLikedSongs()
                {
                    UserId = userId,
                    SongId = songId
                });
                await _songRepository.UpdateAsync(song);
                await _songRepository.SaveChangesAsync();
            }
        }

    }
}
