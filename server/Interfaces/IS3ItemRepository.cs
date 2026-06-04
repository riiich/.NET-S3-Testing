using server.Models;

namespace server.Interfaces;

public interface IS3ItemRepository
{
    Task<S3Item> CreateAsync(S3Item item);

    Task<S3Item> UpdateAsync(S3Item item);

    Task<S3Item?> GetByIdAsync(int id);

    Task<IReadOnlyList<S3Item>> GetByUserIdAsync(int userId);

    Task<S3Item?> UpdateLastRetrievedAsync(int id);
    Task<S3Item?> DeleteById(int userId, int s3ItemId);
}
