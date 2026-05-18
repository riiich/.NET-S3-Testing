using server.Models;

namespace server.Interfaces;

public interface IS3ItemService
{
    Task<S3Item> CreateAsync(CreateS3ItemRequest request);

    Task<S3Item?> GetByIdAsync(int id);

    Task<IReadOnlyList<S3Item>> GetByUserIdAsync(int userId);

    Task<S3Item?> MarkRetrievedAsync(int id);
}
