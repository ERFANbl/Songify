using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.DbMpdels
{
    [Table("SNGF_PlaylistSong")]
    public class PlaylistSong
    {
        public int SongId { get; set; }
        public Song Song { get; set; }

        public int PlaylistId { get; set; }
        public PlayList Playlist { get; set; }

        public int? Position { get; set; } = 0;
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }


}
