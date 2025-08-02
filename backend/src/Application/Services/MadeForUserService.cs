using Application.Interfaces.Services;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Song;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        public async Task<string> CallApiForUser(string WeeklyLogs)
        {
            using var httpClient = new HttpClient();

            var response = await httpClient.PostAsync("", new StringContent(WeeklyLogs));

            return await response.Content.ReadAsStringAsync();
        }

        public async Task UpdateWeeklyRecommendedSongs()
        {
            var users = await _userRepository.GetAllAsync();

            foreach (var user in users)
            {
                var result = await CallApiForUser(user.FourWeekLogsJson);
                user.MadeForUser = result;
                await _userRepository.UpdateAsync(user);
            }

            await _userRepository.SaveChangesAsync();
        }

        public async Task<ICollection<GetSongsMetaDataDTO>?> GetWeeklyRecommendedSongsAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            var MadeForYou = user.MadeForUser;

            List<string> idList = MadeForYou.Split(',')
                                            .ToList();

            return await _mfuRepository.GetAllRecomendedSongsAsync(idList, userId);

        }
    }
}
