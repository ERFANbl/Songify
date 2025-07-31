using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IStreamRepository
    {
        public Task<(Stream stream, long start, long end, long total)> GetAudioRangeAsync(string fileName, string rangeHeader);
    }
}
