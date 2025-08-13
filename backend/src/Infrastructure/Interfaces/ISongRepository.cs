using Application.DTOs.Song;
using Domain.DbMpdels;

namespace Infrastructure.Interfaces
{
    public interface ISongRepository : IRepository<Song>
    {
        public Task UploadToS3Async(Stream songStream, string fileName);
        public Task<ICollection<GetSongsMetaDataDTO>?> GetUserSongsAsync(int userId);
        public Task<GetSongsMetaDataDTO?> GetSongMetadataByIdAsync(int songId, int userId);

    }
}
