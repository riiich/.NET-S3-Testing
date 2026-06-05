using server.Models;

namespace server.Interfaces;

public interface IStoredS3FileService
{
    Task<StoredS3File> CreateAsync(CreateStoredS3FileRequest request);

    Task<StoredS3File?> GetByIdAsync(int id);

    Task<IReadOnlyList<StoredS3File>> GetByUserIdAsync(int userId);
    Task<StoredS3File?> DeleteById(int userId, int storedS3FileId);

    Task<StoredS3File?> MarkRetrievedAsync(int id);
}
