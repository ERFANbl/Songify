using Domain.DbMpdels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ISongRepository : IRepository<Song>
    {
        public Task UploadToS3Async(byte[] songData, string fileName);

        public Task<ICollection<Song>?> GetUserSongsAsync(int userId);

    }
}
