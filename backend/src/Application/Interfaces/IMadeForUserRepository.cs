using Application.DTOs.Song;
using Domain.DbMpdels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IMadeForUserRepository
    {
        public Task<ICollection<GetSongsMetaDataDTO>> GetAllRecomendedSongsAsync(List<string> songIDs, int userId);
    }
}
