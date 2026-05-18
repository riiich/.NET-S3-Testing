using server.Models;

namespace server.Interfaces;

public interface IUserRepository
{
    Task<User> CreateAsync(User user);

    Task<User?> GetByIdAsync(int id);

    Task<User?> GetByNameAndEmailAsync(string name, string email);

    Task<IReadOnlyList<User>> GetAllAsync();
}
