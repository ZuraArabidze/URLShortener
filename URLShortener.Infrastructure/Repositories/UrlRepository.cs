using Microsoft.EntityFrameworkCore;
using URLShortener.Core.Entities;
using URLShortener.Core.Interfaces;
using URLShortener.Infrastructure.Data;

namespace URLShortener.Infrastructure.Repositories;

public class UrlRepository : IUrlRepository
{
    private readonly UrlShortenerDbContext _context;

    public UrlRepository(UrlShortenerDbContext context)
    {
        _context = context;
    }

    public async Task<ShortenedUrl?> GetByShortCodeAsync(string shortCode)
    {
        return await _context.ShortenedUrls.FirstOrDefaultAsync(u => u.ShortCode == shortCode);
    }

    public async Task<ShortenedUrl?> GetByIdAsync(Guid id)
    {
        return await _context.ShortenedUrls.FindAsync(id);
    }

    public async Task<ShortenedUrl> CreateAsync(ShortenedUrl shortenedUrl)
    {
        _context.ShortenedUrls.Add(shortenedUrl);
        await _context.SaveChangesAsync();
        return shortenedUrl;
    }

    public async Task<bool> UpdateAsync(ShortenedUrl shortenedUrl)
    {
       _context.ShortenedUrls.Update(shortenedUrl);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var url = await GetByIdAsync(id);
        if(url == null)
        {
            return false;
        }

        _context.ShortenedUrls.Remove(url);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> ShortCodeExistsAsync(string shortCode)
    {
        return await _context.ShortenedUrls.AnyAsync(u => u.ShortCode == shortCode);
    }

    public async Task<IEnumerable<ShortenedUrl>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
    {
        return await _context.ShortenedUrls
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
