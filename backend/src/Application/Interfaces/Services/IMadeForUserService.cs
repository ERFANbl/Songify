using Application.DTOs.Song;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IMadeForUserService
    {
        public Task UpdateWeeklyRecommendedSongs();
        public Task<ICollection<GetSongsMetaDataDTO>?> GetWeeklyRecommendedSongsAsync(int userId);
    }
}
