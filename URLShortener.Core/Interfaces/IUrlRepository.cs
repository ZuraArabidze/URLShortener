using URLShortener.Core.Entities;

namespace URLShortener.Core.Interfaces;

public interface IUrlRepository
{
    Task<ShortenedUrl?> GetByShortCodeAsync(string shortCode);
    Task<ShortenedUrl?> GetByIdAsync(Guid id);
    Task<ShortenedUrl> CreateAsync (ShortenedUrl shortenedUrl);
    Task<bool> UpdateAsync (ShortenedUrl shortenedUrl);
    Task<bool> DeleteAsync (Guid id);
    Task<bool> ShortCodeExistsAsync (string shortCode);
    Task<IEnumerable<ShortenedUrl>> GetAllAsync (int pageNumber = 1, int pageSize = 10);
}
