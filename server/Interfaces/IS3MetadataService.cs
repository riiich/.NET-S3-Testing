using server.Models;

namespace server.Interfaces;

public interface IS3MetadataService
{
    Task<S3Metadata?> GetByIdAsync(int id);

    Task<S3Metadata?> GetByOwnerIdAndIdAsync(string ownerId, int s3MetadataId);

    Task<IReadOnlyList<S3Metadata>> GetByOwnerIdAsync(string ownerId);

    Task DeleteAsync(S3Metadata item);

    Task<S3Metadata?> MarkRetrievedAsync(int id);
}