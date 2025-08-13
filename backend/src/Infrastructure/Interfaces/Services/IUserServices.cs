namespace Infrastructure.Interfaces.Services
{
    public interface IUserServices
    {
        Task<int?> GetUserIdByTokenAsync(string token);
    }
}
