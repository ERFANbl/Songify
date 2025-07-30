using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DbMpdels
{
    [Table("SNGF_LikedSongs")]
    public class UserLikedSongs
    {
        public int UserId {  get; set; }
        public User User { get; set; }
        public int SongId { get; set; }
        public Song Song { get; set; }
        public DateTime LikedAt { get; set; }
    }
}
