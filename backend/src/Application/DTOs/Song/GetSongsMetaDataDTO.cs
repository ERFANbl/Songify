using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Song
{
    public class GetSongsMetaDataDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Artist { get; set; }
        public TimeSpan TrackDuration { get; set; }
        public string? Lyric { get; set; }
        public string? ForigenKey { get; set; }
        public string? Genre { get; set; }
        public string? ReleaseDate { get; set; }
    }
}
