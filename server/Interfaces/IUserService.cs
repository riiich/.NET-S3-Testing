using server.Models;

namespace server.Interfaces;

public interface IUserService
{
    Task<User> CreateAsync(CreateUserRequest request);

    Task<User?> GetByIdAsync(int id);

    Task<User?> LoginAsync(LoginUserRequest request);

    Task<IReadOnlyList<User>> GetAllAsync();
}
