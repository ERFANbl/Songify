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
        public string? Genre { get; set; }
        public string? ReleaseDate { get; set; }
        public string? ForigenKey { get; set; }
        public bool is_deleted { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }


}
