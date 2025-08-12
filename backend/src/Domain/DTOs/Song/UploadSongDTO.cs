using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Song
{
    public class UploadSongDTO
    {
        public string Name { get; set; }
        public string? Artist { get; set; }
        public TimeSpan TrackDuration { get; set; }
        public string? Lyric { get; set; }
        public string? Genre { get; set; }
        public string? ReleaseDate { get; set; }
        public IFormFile audioData { get; set; }
    }


}
