using server.Interfaces;
using server.Models;

namespace server.Services;

public class StoredS3FileService : IStoredS3FileService
{
    private readonly IStoredS3FileRepository _storedS3FileRepository;
    private readonly IUserRepository _userRepository;

    public StoredS3FileService(IStoredS3FileRepository storedS3FileRepository, IUserRepository userRepository)
    {
        _storedS3FileRepository = storedS3FileRepository;
        _userRepository = userRepository;
    }

    public async Task<StoredS3File> CreateAsync(CreateStoredS3FileRequest request)
    {
        User? user = await _userRepository.GetByIdAsync(request.UserId);

        if (user is null)
        {
            throw new ArgumentException("User was not found.");
        }

        StoredS3File item = new()
        {
            UserId = request.UserId,
            S3Key = request.S3Key,
            Status = StoredS3FileStatus.Uploaded,
            UploadedAt = DateTime.UtcNow,
            FileName = request.FileName,
            MimeType = request.MimeType,
            FileSize = request.FileSize
        };

        return await _storedS3FileRepository.CreateAsync(item);
    }

    public async Task<StoredS3File?> GetByIdAsync(int id)
    {
        return await _storedS3FileRepository.GetByIdAsync(id);
    }

    public async Task<IReadOnlyList<StoredS3File>> GetByUserIdAsync(int userId)
    {
        return await _storedS3FileRepository.GetByUserIdAsync(userId);
    }

    public async Task<StoredS3File?> DeleteById(int userId, int storedS3FileId)
    {
        return await _storedS3FileRepository.DeleteById(userId, storedS3FileId);
    }

    public async Task<StoredS3File?> MarkRetrievedAsync(int id)
    {
        return await _storedS3FileRepository.UpdateLastRetrievedAsync(id);
    }
}
