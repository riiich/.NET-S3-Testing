using server.Models;

namespace server.Interfaces;

public interface IFileUploadWorkflowService
{
    Task<S3Metadata> UploadAsync(string ownerId, FileUploadInput file);
}