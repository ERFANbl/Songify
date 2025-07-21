using Application.Interfaces;
using Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class StreamService : IStreamService
    {

        private readonly IStreamRepository _streamRepository;

        public StreamService(IStreamRepository streamRepository)
        {
            _streamRepository = streamRepository;
        }

        public async Task<(Stream stream, long start, long end, long total)> GetAudioChunkAsync(string fileName, string range)
        {
            return await _streamRepository.GetAudioRangeAsync(fileName, range);
        }
    }

}
