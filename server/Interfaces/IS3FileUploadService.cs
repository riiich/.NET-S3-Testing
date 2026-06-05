using server.Models;

namespace server.Interfaces
{
    public interface IS3FileUploadService
    {
        string CreateObjectKey(string fileName);
        Task UploadFileAsync(FileUploadInput file, string s3Key);
        Task DeleteFileAsync(string s3Key);
        string GetPresignedUrl(string s3Key);
    }
}
