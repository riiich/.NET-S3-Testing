using server.Interfaces;
using server.Models;

namespace server.Services;

public class S3UploadService : IS3UploadService
{
    private readonly IS3FileStorageService _storageService;
    private readonly IS3ItemRepository _s3ItemRepository;
    private readonly IUserRepository _userRepository;

    public S3UploadService(
        IS3FileStorageService storageService,
        IS3ItemRepository s3ItemRepository,
        IUserRepository userRepository)
    {
        _storageService = storageService;
        _s3ItemRepository = s3ItemRepository;
        _userRepository = userRepository;
    }

    public async Task<S3Item> UploadAsync(int userId, FileUploadInput file)
    {
        User? user = await _userRepository.GetByIdAsync(userId);

        if (user is null)
        {
            throw new ArgumentException("Create an account before uploading files.");
        }

        string s3Key = _storageService.CreateObjectKey(file.FileName);

        S3Item item = new()
        {
            UserId = userId,
            S3Key = s3Key,
            Status = S3ItemStatus.Pending,
            FileName = file.FileName,
            MimeType = file.ContentType,
            FileSize = file.Length
        };

        await _s3ItemRepository.CreateAsync(item);

        try
        {
            await _storageService.UploadFileAsync(file, s3Key);
        }
        catch
        {
            item.Status = S3ItemStatus.Failed;
            try
            {
                await _s3ItemRepository.UpdateAsync(item);
            }
            catch
            {
                // Preserve the original storage failure if the status update also fails.
            }

            throw;
        }

        item.Status = S3ItemStatus.Uploaded;
        item.UploadedAt = DateTime.UtcNow;

        return await _s3ItemRepository.UpdateAsync(item);
    }
}
