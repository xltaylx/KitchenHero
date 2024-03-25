using Data.Models;

namespace AuthAPI.Models
{
    public interface ITokenService
    {
        Task<string> GenerateAccessTokenAsync(User user);
        Task<string> GenerateRefreshTokenAsync(User userId);
        Task<bool> ValidateAccessTokenAsync(string accessToken);
    }
}
