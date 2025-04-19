using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.DbMpdels
{
    [Table("SNGF_Song")]
    public class Song
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Artist { get; set; }
        public TimeSpan TrackDuration { get; set; }
        public string? Lyric { get; set; }
        public string? MetaData { get; set; }

        public List<PlaylistSong>? PlaylistSongs { get; set; } // Many-to-Many

    }


}
