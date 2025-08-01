using Application.Interfaces.Services;
using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class MadeForUserService : IMadeForUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ISongRepository _songRepository;

        public MadeForUserService(IUserRepository userRepository, ITokenService tokenService, ISongRepository songRepository)
        {
            _userRepository = userRepository;
            _songRepository = songRepository;
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

    }
}
