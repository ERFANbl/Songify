using Domain.DTOs.UserVector;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class UserVectorService : IUserVectorService
    {
        private readonly IConfiguration _configService;
        private readonly IUserRepository _userRepository;

        public UserVectorService(IConfiguration configService, IUserRepository userRepository) 
        {
            _configService = configService;
            _userRepository = userRepository;
        }

        private async Task CallUserVectorService(string vectorId, UserInteractionsDTO userInteractions)
        {
            using var client = new HttpClient();
            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(userInteractions.WeeklyUserInteractions ?? ""), "WeeklyUserInteractions");
            content.Add(new StringContent(userInteractions.latestUserInteractions ?? ""), "latestUserInteractions");

            await client.PostAsync($"http://{_configService["RecommenderServices:UserVectorService:Host"]}:{_configService["RecommenderServices:UserVectorService:Port"]}/UpdateUserVector/{vectorId}", content);
        }

        public async Task UpdateUserVectorAsync(int userId, UserInteractionsDTO userInteractions)
        {
            await _userRepository.UpdateWeeklyLogsAsync(userId, userInteractions.WeeklyUserInteractions);

            var user = await _userRepository.GetByIdAsync(userId);

            await CallUserVectorService(user.UserVectorId, userInteractions);
        }
    }
}
