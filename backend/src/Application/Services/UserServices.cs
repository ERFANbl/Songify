using Infrastructure.Interfaces;
using Infrastructure.Interfaces.Services;

namespace Application.Services
{
    public class UserServices : IUserServices
    {
        private readonly IUserRepository _userRepository;

        public UserServices(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<int?> GetUserIdByTokenAsync(string token) =>
            await _userRepository.GetUserIdByTokenAsync(token);

    }
}
