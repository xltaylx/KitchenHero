using DataModels.Models;
using DataModels.Repositories;

namespace AuthAPI.Models
{
  public interface IUserService
{
    Task<User> GetUserByUsername(string username);
}

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository; // Interface for data access logic

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User> GetUserByUsername(string username)
    {
        return await _userRepository.GetUserByUsername(username);
    }
}
}
