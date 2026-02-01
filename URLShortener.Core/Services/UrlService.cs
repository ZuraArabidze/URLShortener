using System.Security.Cryptography;
using URLShortener.Core.Entities;
using URLShortener.Core.Exceptions;
using URLShortener.Core.Interfaces;

namespace URLShortener.Core.Services;

public class UrlService : IUrlService
{
    private readonly IUrlRepository _urlRepository;
    private readonly ICacheService _cacheService;
    private const int ShortCodeLength = 8;
    private const string CacheKeyPrefix = "url:";

    public UrlService(IUrlRepository urlRepository, ICacheService cacheService)
    {
        _urlRepository = urlRepository;
        _cacheService = cacheService;
    }
    public async Task<ShortenedUrl> CreateShortUrlAsync(string originalUrl, string? customShortCode = null, DateTime? expiresAt = null, string? createdByIp = null)
    {
        if(!IsValid(originalUrl))
        {
            throw new InvalidUrlException(originalUrl);
        }

        string shortCode;

        if(!string.IsNullOrWhiteSpace(customShortCode))
        {
            if(await _urlRepository.ShortCodeExistsAsync(customShortCode))
            {
                throw new ShortCodeAlreadyExistsException(customShortCode);
            }

            shortCode = customShortCode;
        }
        else
        {
            shortCode = await GenerateUniqueShortCodeAsync();
        }

        var shortenedUrl = new ShortenedUrl
        {
            Id = Guid.NewGuid(),
            OriginalUrl = originalUrl,
            ShortCode = shortCode,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            ClickCount = 0,
            CreatedByIp = createdByIp
        };

        var result = await _urlRepository.CreateAsync(shortenedUrl);

        await _cacheService.SetAsync(
            $"{CacheKeyPrefix}-{shortCode}",
            result,
            TimeSpan.FromHours(1));

        return result;
    }

    public async Task<string?> GetOriginalUrlAsync(string shortCode)
    {
        var cachedResult = await _cacheService.GetAsync<ShortenedUrl>($"{CacheKeyPrefix}-{shortCode}");

        ShortenedUrl? shortenedUrl;

        if (cachedResult != null)
        {
            shortenedUrl = cachedResult;
        }
        else
        {
            shortenedUrl = await _urlRepository.GetByShortCodeAsync(shortCode);
            if (shortenedUrl == null)
                throw new UrlNotFoundException(shortCode);

            await _cacheService.SetAsync(
                $"{CacheKeyPrefix}-{shortCode}",
                shortenedUrl,
                TimeSpan.FromHours(1));
        }

        if (shortenedUrl.ExpiresAt != null && shortenedUrl.ExpiresAt <= DateTime.UtcNow)
        {
            throw new UrlExpiredException(shortCode);
        }
        
        shortenedUrl.ClickCount++;
        await _urlRepository.UpdateAsync(shortenedUrl);

        await _cacheService.SetAsync(
            $"{CacheKeyPrefix}-{shortCode}",
            shortenedUrl,
            TimeSpan.FromHours(1));

        return shortenedUrl.OriginalUrl;
    }

    public async Task<ShortenedUrl?> GetShortenedUrlDetailsAsync(string shortCode)
    {
       var cachedUrl = await _cacheService.GetAsync<ShortenedUrl>($"{CacheKeyPrefix}-{shortCode}");
       
       if(cachedUrl != null)
       {
            return cachedUrl;
       }

       var url = await _urlRepository.GetByShortCodeAsync(shortCode);

       if(url != null)
       {
            await _cacheService.SetAsync(
                $"{CacheKeyPrefix}-{shortCode}",
                url,
                TimeSpan.FromHours(1));
       }

       return url;
    }

    public async Task<bool> DeleteShortUrlAsync(Guid id)
    {
        var url = await _urlRepository.GetByIdAsync(id);

        if (url == null)
            return false;

        await _cacheService.RemoveAsync($"{CacheKeyPrefix}-{url.ShortCode}");

        return await _urlRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<ShortenedUrl>> GetAllShortenedUrlsAsync(int pageNumber = 1, int pageSize = 10)
    {
        return await _urlRepository.GetAllAsync(pageNumber, pageSize);
    }
    
    private async Task<string> GenerateUniqueShortCodeAsync()
    {
        const int maxAttempts = 10;

        for (int i = 0; i < maxAttempts; i++)
        {
            var shortCode = GenerateShortCode();

            if (!await _urlRepository.ShortCodeExistsAsync(shortCode))
                return shortCode;
        }

        throw new UrlShortenerException("Failed to generate unique short code after multiple attempts.");
    }

    private string GenerateShortCode()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var shortCode = new char[ShortCodeLength];

        for(int i = 0; i< ShortCodeLength; i++)
        {
            shortCode[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
        }

        return new string(shortCode);
    }

    private bool IsValid(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
