using Application.DTOs.Song;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Services;

namespace Application.Services
{
    public class MadeForUserService : IMadeForUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMadeForUserRepository _mfuRepository;

        public MadeForUserService(IUserRepository userRepository, ITokenService tokenService, IMadeForUserRepository mfuRepository)
        {
            _userRepository = userRepository;
            _mfuRepository = mfuRepository;
        }

        public async Task<string> CallApiMadeForUser(string WeeklyLogs, string UserVectorId)
        {
            using var httpClient = new HttpClient();
            using var formContent = new MultipartFormDataContent();

            formContent.Add(new StringContent(WeeklyLogs), "WeeklyLogs");
            formContent.Add(new StringContent(UserVectorId), "UserVectorId");

            var response = await httpClient.PostAsync("", formContent);

            return await response.Content.ReadAsStringAsync();
        }

        public async Task UpdateWeeklyRecommendedSongs()
        {
            var users = await _userRepository.GetAllAsync();

            foreach (var user in users)
            {
                var result = await CallApiMadeForUser(user.FourWeekLogsJson, user.UserVectorId);
                user.MadeForUser = result;
                await _userRepository.UpdateAsync(user);
            }

            await _userRepository.SaveChangesAsync();
        }

        public async Task<ICollection<GetSongsMetaDataDTO>?> GetWeeklyRecommendedSongsAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            var MadeForYou = user.MadeForUser;

            List<string> idList;
            if (MadeForYou != null)
            {
                idList = MadeForYou.Split(',')
                                   .ToList();
            }
            else 
                idList = new List<string>();

            return await _mfuRepository.GetAllRecomendedSongsAsync(idList, userId);

        }
    }
}
