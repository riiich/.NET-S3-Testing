using server.Interfaces;
using server.Models;

namespace server.Services;

public class S3ItemService : IS3ItemService
{
    private readonly IS3ItemRepository _s3ItemRepository;
    private readonly IUserRepository _userRepository;

    public S3ItemService(IS3ItemRepository s3ItemRepository, IUserRepository userRepository)
    {
        _s3ItemRepository = s3ItemRepository;
        _userRepository = userRepository;
    }

    public async Task<S3Item> CreateAsync(CreateS3ItemRequest request)
    {
        User? user = await _userRepository.GetByIdAsync(request.UserId);

        if (user is null)
        {
            throw new ArgumentException("User was not found.");
        }

        S3Item item = new()
        {
            UserId = request.UserId,
            S3Key = request.S3Key,
            UploadedAt = DateTime.UtcNow,
            FileName = request.FileName,
            MimeType = request.MimeType,
            FileSize = request.FileSize
        };

        return await _s3ItemRepository.CreateAsync(item);
    }

    public async Task<S3Item?> GetByIdAsync(int id)
    {
        return await _s3ItemRepository.GetByIdAsync(id);
    }

    public async Task<IReadOnlyList<S3Item>> GetByUserIdAsync(int userId)
    {
        return await _s3ItemRepository.GetByUserIdAsync(userId);
    }

    public async Task<S3Item?> MarkRetrievedAsync(int id)
    {
        return await _s3ItemRepository.UpdateLastRetrievedAsync(id);
    }
}
