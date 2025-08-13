using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Services;

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
