using server.Interfaces;
using server.Models;

namespace server.Services;

public class FileUploadWorkflowService : IFileUploadWorkflowService
{
    private readonly ILogger<FileUploadWorkflowService> _logger;
    private readonly IS3FileUploadService _storageService;
    private readonly IStoredS3FileRepository _storedS3FileRepository;
    private readonly IUserRepository _userRepository;

    public FileUploadWorkflowService(
        ILogger<FileUploadWorkflowService> logger,
        IS3FileUploadService storageService,
        IStoredS3FileRepository storedS3FileRepository,
        IUserRepository userRepository)
    {
        _logger = logger;
        _storageService = storageService;
        _storedS3FileRepository = storedS3FileRepository;
        _userRepository = userRepository;
    }
    
    public async Task<StoredS3File> UploadAsync(int userId, FileUploadInput file)
    {
        User? user = await _userRepository.GetByIdAsync(userId);

        if (user is null)
        {
            throw new ArgumentException("Create an account before uploading files.");
        }

        string s3Key = _storageService.CreateObjectKey(file.FileName);

        StoredS3File item = new()
        {
            UserId = userId,
            S3Key = s3Key,
            Status = StoredS3FileStatus.Pending,
            FileName = file.FileName,
            MimeType = file.ContentType,
            FileSize = file.Length
        };

        await _storedS3FileRepository.CreateAsync(item);

        try
        {
            await _storageService.UploadFileAsync(file, s3Key);
        }
        catch
        {
            item.Status = StoredS3FileStatus.Failed;
            try
            {
                await _storedS3FileRepository.UpdateAsync(item);
            }
            catch (Exception statusUpdateException)
            {
                _logger.LogError(
                    statusUpdateException,
                    "Failed to mark S3 item {StoredS3FileId} as failed after upload error. S3 key: {S3Key}",
                    item.Id,
                    item.S3Key);
            }

            throw;
        }

        item.Status = StoredS3FileStatus.Uploaded;
        item.UploadedAt = DateTime.UtcNow;

        return await _storedS3FileRepository.UpdateAsync(item);
    }
}
