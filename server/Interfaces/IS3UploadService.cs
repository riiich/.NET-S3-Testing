using server.Models;

namespace server.Interfaces;

public interface IS3UploadService
{
    Task<S3Item> UploadAsync(int userId, FileUploadInput file);
}
