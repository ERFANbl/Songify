using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Interfaces.Services
{
    public interface IStreamService
    {
        public Task<(Stream stream, long start, long end, long total)> GetAudioChunkAsync(string fileName, string range);
    }
}
