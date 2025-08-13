using Application.DTOs.Song;

namespace Infrastructure.Interfaces
{
    public interface IMadeForUserRepository
    {
        public Task<ICollection<GetSongsMetaDataDTO>> GetAllRecomendedSongsAsync(List<string> songIDs, int userId);
    }
}
