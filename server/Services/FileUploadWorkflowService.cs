using server.Interfaces;
using server.Models;

namespace server.Services;

public class FileUploadWorkflowService : IFileUploadWorkflowService
{
    private readonly ILogger<FileUploadWorkflowService> _logger;
    private readonly IS3FileUploadService _storageService;
    private readonly IS3MetadataRepository _s3MetadataRepository;

    public FileUploadWorkflowService(
        ILogger<FileUploadWorkflowService> logger,
        IS3FileUploadService storageService,
        IS3MetadataRepository s3MetadataRepository)
    {
        _logger = logger;
        _storageService = storageService;
        _s3MetadataRepository = s3MetadataRepository;
    }
    
    public async Task<S3Metadata> UploadAsync(string ownerId, FileUploadInput file)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
        {
            throw new ArgumentException("An owner id is required before uploading files.");
        }

        string s3Key = _storageService.CreateObjectKey(file.FileName);

        S3Metadata item = new()
        {
            OwnerId = ownerId.Trim(),
            S3Key = s3Key,
            Status = S3MetadataStatus.Pending,
            FileName = file.FileName,
            MimeType = file.ContentType,
            FileSize = file.Length
        };

        await _s3MetadataRepository.CreateAsync(item);

        try
        {
            await _storageService.UploadFileAsync(file, s3Key);
        }
        catch
        {
            item.Status = S3MetadataStatus.Failed;
            try
            {
                await _s3MetadataRepository.UpdateAsync(item);
            }
            catch (Exception statusUpdateException)
            {
                _logger.LogError(
                    statusUpdateException,
                    "Failed to mark S3 metadata {S3MetadataId} as failed after upload error. S3 key: {S3Key}",
                    item.Id,
                    item.S3Key);
            }

            throw;
        }

        item.Status = S3MetadataStatus.Uploaded;
        item.UploadedAt = DateTime.UtcNow;

        return await _s3MetadataRepository.UpdateAsync(item);
    }
}