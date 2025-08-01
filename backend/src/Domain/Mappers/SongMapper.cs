using Application.DTOs.Song;
using Domain.DbMpdels;

namespace Application.Mappers
{
    public static class SongMapper
    {
        // Map from Song to GetSongsMetaDataDTO
        public static GetSongsMetaDataDTO MapToGetSongsMetaDataDTO(Song song, bool isLiked = false)
        {
            if (song == null) return null;

            return new GetSongsMetaDataDTO
            {
                Id = song.Id,
                Name = song.Name,
                Artist = song.Artist,
                TrackDuration = song.TrackDuration,
                Lyric = song.Lyric,
                Genre = song.Genre,
                ReleaseDate = song.ReleaseDate,
                ForigenKey = song.ForigenKey,
                Isliked = isLiked
            };
        }

        // Map from UploadSongDTO to Song
        public static Song MapFromUploadSongDTO(UploadSongDTO uploadSongDto, int userId)
        {
            if (uploadSongDto == null) return null;

            return new Song
            {
                Name = uploadSongDto.Name,
                Artist = uploadSongDto.Artist,
                TrackDuration = uploadSongDto.TrackDuration,
                Lyric = uploadSongDto.Lyric,
                Genre = uploadSongDto.Genre,
                ReleaseDate = uploadSongDto.ReleaseDate,
                is_deleted = false,
                UserId = userId,
                ForigenKey = null 
            };
        }

        // Map from Song to UploadSongDTO (if needed)
        public static UploadSongDTO MapToUploadSongDTO(Song song)
        {
            if (song == null) return null;

            return new UploadSongDTO
            {
                Name = song.Name,
                Artist = song.Artist,
                TrackDuration = song.TrackDuration,
                Lyric = song.Lyric,
                Genre = song.Genre,
                ReleaseDate = song.ReleaseDate
            };
        }

        // Map from GetSongsMetaDataDTO to Song (if needed)
        public static Song MapFromGetSongsMetaDataDTO(GetSongsMetaDataDTO dto, int userId)
        {
            if (dto == null) return null;

            return new Song
            {
                Id = dto.Id,
                Name = dto.Name,
                Artist = dto.Artist,
                TrackDuration = dto.TrackDuration,
                Lyric = dto.Lyric,
                Genre = dto.Genre,
                ReleaseDate = dto.ReleaseDate,
                ForigenKey = dto.ForigenKey,
                is_deleted = false,
                UserId = userId
            };
        }
    }
}
