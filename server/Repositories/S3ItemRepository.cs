using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Interfaces;
using server.Models;

namespace server.Repositories;

public class S3ItemRepository : IS3ItemRepository
{
    private readonly ApplicationDbContext _context;

    public S3ItemRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<S3Item> CreateAsync(S3Item item)
    {
        _context.S3Items.Add(item);
        await _context.SaveChangesAsync();

        return item;
    }

    public async Task<S3Item?> GetByIdAsync(int id)
    {
        return await _context.S3Items
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);
    }

    public async Task<IReadOnlyList<S3Item>> GetByUserIdAsync(int userId)
    {
        return await _context.S3Items
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.UploadedAt)
            .ToListAsync();
    }

    public async Task<S3Item?> UpdateLastRetrievedAsync(int id)
    {
        S3Item? item = await _context.S3Items.FirstOrDefaultAsync(s3Item => s3Item.Id == id);

        if (item is null)
        {
            return null;
        }

        item.LastRetrieved = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return item;
    }
}
