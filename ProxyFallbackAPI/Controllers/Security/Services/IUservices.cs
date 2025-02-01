namespace ProxyFallbackAPI.Services
{
    public interface IUserService
    {
        Task<bool> ValidateUserAsync(string username, string password);
    }
}
