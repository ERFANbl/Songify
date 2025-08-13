using Application.DTOs.Song;
using Domain.DbMpdels;

namespace Infrastructure.Interfaces
{
    public interface ISongRepository : IRepository<Song>
    {
        public Task UploadToS3Async(byte[] songData, string fileName);

        Task<ICollection<GetSongsMetaDataDTO>?> GetUserSongsAsync(int userId);

    }
}
