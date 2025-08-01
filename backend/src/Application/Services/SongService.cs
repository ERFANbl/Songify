using Application.DTOs.Song;
using Application.Interfaces;
using Application.Interfaces.Services;
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
