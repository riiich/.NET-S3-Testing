using server.Models;

namespace server.Interfaces;

public interface IStoredS3FileRepository
{
    Task<StoredS3File> CreateAsync(StoredS3File item);

    Task<StoredS3File> UpdateAsync(StoredS3File item);

    Task<StoredS3File?> GetByIdAsync(int id);

    Task<IReadOnlyList<StoredS3File>> GetByUserIdAsync(int userId);

    Task<StoredS3File?> UpdateLastRetrievedAsync(int id);
    Task<StoredS3File?> DeleteById(int userId, int storedS3FileId);
}
