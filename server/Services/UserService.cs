using server.Interfaces;
using server.Models;

namespace server.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User> CreateAsync(CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email is required.");
        }

        User user = new()
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        return await _userRepository.CreateAsync(user);
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task<User?> LoginAsync(LoginUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Email is required.");
        }

        return await _userRepository.GetByNameAndEmailAsync(
            request.Name.Trim(),
            request.Email.Trim());
    }

    public async Task<IReadOnlyList<User>> GetAllAsync()
    {
        return await _userRepository.GetAllAsync();
    }
}
