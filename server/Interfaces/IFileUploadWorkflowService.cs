using server.Models;

namespace server.Interfaces;

public interface IFileUploadWorkflowService
{
    Task<StoredS3File> UploadAsync(int userId, FileUploadInput file);
}
