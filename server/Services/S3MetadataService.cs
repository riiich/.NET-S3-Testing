using server.Interfaces;
using server.Models;

namespace server.Services;

public class S3MetadataService : IS3MetadataService
{
    private readonly IS3MetadataRepository _s3MetadataRepository;

    public S3MetadataService(IS3MetadataRepository s3MetadataRepository)
    {
        _s3MetadataRepository = s3MetadataRepository;
    }

    public async Task<S3Metadata?> GetByIdAsync(int id)
    {
        return await _s3MetadataRepository.GetByIdAsync(id);
    }

    public async Task<S3Metadata?> GetByOwnerIdAndIdAsync(string ownerId, int s3MetadataId)
    {
        return await _s3MetadataRepository.GetByOwnerIdAndIdAsync(ownerId, s3MetadataId);
    }

    public async Task<IReadOnlyList<S3Metadata>> GetByOwnerIdAsync(string ownerId)
    {
        return await _s3MetadataRepository.GetByOwnerIdAsync(ownerId);
    }

    public async Task DeleteAsync(S3Metadata item)
    {
        await _s3MetadataRepository.DeleteAsync(item);
    }

    public async Task<S3Metadata?> MarkRetrievedAsync(int id)
    {
        return await _s3MetadataRepository.UpdateLastRetrievedAsync(id);
    }
}