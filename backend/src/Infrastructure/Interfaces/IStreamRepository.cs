namespace Infrastructure.Interfaces
{
    public interface IStreamRepository
    {
        public Task<(Stream stream, long start, long end, long total)> GetAudioRangeAsync(string fileName, string rangeHeader);
    }
}
