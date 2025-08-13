using Application.DTOs.Song;
using Domain.DbMpdels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.Services
{
    public interface ISongService
    {
        public  Task<string> UploadSongAsync(byte[] audioData, UploadSongDTO song, int userId);

        public  Task<string> DeleteSongAsync(int userId, int songId);

        public Task<ICollection<GetSongsMetaDataDTO>?> GetAllSongsMetadataAsync(int userId);

        public Task<GetSongsMetaDataDTO?> GetSongMetadataByIdAsync(int songId);
        Task LikeSong(int userId, int songId);
    }
}
