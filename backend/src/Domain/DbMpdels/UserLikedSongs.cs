using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.DbMpdels
{
    [Table("SNGF_LikedSongs")]
    public class UserLikedSongs
    {
        public int Id { get; set; }
        public int UserId {  get; set; }
        public User User { get; set; }
        public int SongId { get; set; }
        public Song Song { get; set; }
        public DateTime LikedAt { get; set; }
    }
}
