using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Interfaces;
using server.Models;

namespace server.Repositories;

public class StoredS3FileRepository : IStoredS3FileRepository
{
    private readonly ApplicationDbContext _context;

    public StoredS3FileRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StoredS3File> CreateAsync(StoredS3File item)
    {
        _context.StoredS3Files.Add(item);
        await _context.SaveChangesAsync();

        return item;
    }

    public async Task<StoredS3File> UpdateAsync(StoredS3File item)
    {
        await _context.SaveChangesAsync();

        return item;
    }

    public async Task<StoredS3File?> GetByIdAsync(int id)
    {
        return await _context.StoredS3Files
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);
    }

    public async Task<IReadOnlyList<StoredS3File>> GetByUserIdAsync(int userId)
    {
        return await _context.StoredS3Files
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.UploadedAt)
            .ToListAsync();
    }

    public async Task<StoredS3File?> UpdateLastRetrievedAsync(int id)
    {
        StoredS3File? item = await _context.StoredS3Files.FirstOrDefaultAsync(storedS3File => storedS3File.Id == id);

        if (item is null)
        {
            return null;
        }

        item.LastRetrieved = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return item;
    }

    public async Task<StoredS3File?> DeleteById(int userId, int storedS3FileId)
    {
        StoredS3File? item = await _context.StoredS3Files.FirstOrDefaultAsync(s => s.UserId == userId && s.Id == storedS3FileId);

        if(item is null) return null;

        _context.StoredS3Files.Remove(item);
        await _context.SaveChangesAsync();

        return item;
    }
}
