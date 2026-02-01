using URLShortener.Core.Entities;

namespace URLShortener.Core.Interfaces;

public interface IUrlService
{
    Task<ShortenedUrl> CreateShortUrlAsync(string originalUrl, string? customShortCode = null, DateTime? expiresAt = null, string? createdByIp = null);
    Task<string?> GetOriginalUrlAsync(string shortCode);
    Task<ShortenedUrl?> GetShortenedUrlDetailsAsync(string shortCode);
    Task<bool> DeleteShortUrlAsync(Guid id);
    Task<IEnumerable<ShortenedUrl>> GetAllShortenedUrlsAsync(int pageNumber = 1, int pageSize = 10);
}
