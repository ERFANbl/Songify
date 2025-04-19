using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.DbMpdels
{
    [Table("SNGF_PlayList")]
    public class PlayList
    {
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; } // Foreign Key
        public User User { get; set; }   // Navigation Property

        public List<PlaylistSong>? PlaylistSongs { get; set; } // Many-to-Many

    }


}
