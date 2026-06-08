using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Interfaces;
using server.Models;

namespace server.Repositories;

public class S3MetadataRepository : IS3MetadataRepository
{
    private readonly ApplicationDbContext _context;

    public S3MetadataRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<S3Metadata> CreateAsync(S3Metadata item)
    {
        _context.S3Metadata.Add(item);
        await _context.SaveChangesAsync();

        return item;
    }

    public async Task<S3Metadata> UpdateAsync(S3Metadata item)
    {
        await _context.SaveChangesAsync();

        return item;
    }

    public async Task<S3Metadata?> GetByIdAsync(int id)
    {
        return await _context.S3Metadata
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);
    }

    public async Task<S3Metadata?> GetByOwnerIdAndIdAsync(string ownerId, int s3MetadataId)
    {
        return await _context.S3Metadata
            .FirstOrDefaultAsync(item => item.OwnerId == ownerId && item.Id == s3MetadataId);
    }

    public async Task<IReadOnlyList<S3Metadata>> GetByOwnerIdAsync(string ownerId)
    {
        return await _context.S3Metadata
            .AsNoTracking()
            .Where(item => item.OwnerId == ownerId)
            .OrderByDescending(item => item.UploadedAt)
            .ToListAsync();
    }

    public async Task<S3Metadata?> UpdateLastRetrievedAsync(int id)
    {
        S3Metadata? item = await _context.S3Metadata.FirstOrDefaultAsync(s3Metadata => s3Metadata.Id == id);

        if (item is null)
        {
            return null;
        }

        item.LastRetrieved = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return item;
    }

    public async Task DeleteAsync(S3Metadata item)
    {
        _context.S3Metadata.Remove(item);
        await _context.SaveChangesAsync();
    }
}