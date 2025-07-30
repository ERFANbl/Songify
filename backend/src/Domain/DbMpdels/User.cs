using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.DbMpdels
{
    [Table("SNGF_User")]
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        //Updated by Recommendation System 
        public string? MadeForUser { get; set; }
        public string? Token { get; set; }
        public string FourWeekLogsJson { get; set; } = string.Empty;
        public DateTime DateLimit { get; set; }
        public ICollection<Song> Songs { get; set; }
        public ICollection<UserLikedSongs> LikedSongs { get; set; }

    }
}
