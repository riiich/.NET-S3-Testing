using server.Models;

namespace server.Interfaces
{
    public interface IS3FileStorageService
    {
        string CreateObjectKey(string fileName);
        Task UploadFileAsync(FileUploadInput file, string s3Key);
        Task DeleteFileAsync(string s3Key);
        string GetFileUrl(string s3Key);
    }
}
