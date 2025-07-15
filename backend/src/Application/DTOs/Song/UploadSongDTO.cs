using Domain.DbMpdels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Song
{
    public class UploadSongDTO
    {
        public string Name { get; set; }
        public string? Artist { get; set; }
        public TimeSpan TrackDuration { get; set; }
        public string? Lyric { get; set; }
        public string? MetaData { get; set; }
    }
}
