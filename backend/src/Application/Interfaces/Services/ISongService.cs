using Application.DTOs.Song;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface ISongService
    {
        public  Task<string> UploadSongAsync(byte[] audioData, UploadSongDTO song, int userId);
    }
}
