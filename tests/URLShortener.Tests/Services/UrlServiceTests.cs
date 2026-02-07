using Moq;
using URLShortener.Core.Entities;
using URLShortener.Core.Exceptions;
using URLShortener.Core.Interfaces;
using URLShortener.Core.Services;

namespace URLShortener.Tests.Services;

public class UrlServiceTests
{
    private readonly Mock<IUrlRepository> _urlRepositoryMock;
    private readonly Mock<ICacheService> _urlCacheServiceMock;
    private readonly UrlService _urlService;

    public UrlServiceTests()
    {
        _urlRepositoryMock = new Mock<IUrlRepository>();
        _urlCacheServiceMock = new Mock<ICacheService>();
        _urlService = new UrlService(_urlRepositoryMock.Object, _urlCacheServiceMock.Object);
    }

    [Fact]
    public async Task CreateShortUrlAsync_ValidUrl_ReturnShortUrl()
    {
        var originalUrl = "https://www.example.com";
        _urlRepositoryMock.Setup(r => r.ShortCodeExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        _urlRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<ShortenedUrl>()))
            .ReturnsAsync((ShortenedUrl url) => url);

        // Act
        var result = await _urlService.CreateShortUrlAsync(originalUrl);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(originalUrl, result.OriginalUrl);
        Assert.NotEmpty(result.ShortCode);
        Assert.Equal(8, result.ShortCode.Length);
        _urlRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<ShortenedUrl>()), Times.Once);
        _urlCacheServiceMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<ShortenedUrl>(), It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task CreateShortUrlAsync_InvalidUrl_ThrowsInvalidUrlException()
    {
        var invalidUrl = "not-a-valid-url";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidUrlException>(() =>
            _urlService.CreateShortUrlAsync(invalidUrl));
    }

    [Fact]
    public async Task CreateShortUrlAsync_CustomShortCode_UsesCustomCode()
    {
        var originalUrl = "https://www.example.com";
        var customShortCode = "custom";
        _urlRepositoryMock.Setup(r => r.ShortCodeExistsAsync(customShortCode))
            .ReturnsAsync(false);
        _urlRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<ShortenedUrl>()))
            .ReturnsAsync((ShortenedUrl url) => url);

        var result = await _urlService.CreateShortUrlAsync(originalUrl, customShortCode);

        // Assert
        Assert.Equal(customShortCode, result.ShortCode);
    }

    [Fact]
    public async Task CreateShortUrlAsync_DuplicateCustomShortCode_ThrowsException()
    {
        var originalUrl = "https://www.example.com";
        var customShortCode = "existing";
        _urlRepositoryMock.Setup(r => r.ShortCodeExistsAsync(customShortCode))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ShortCodeAlreadyExistsException>(() =>
            _urlService.CreateShortUrlAsync(originalUrl, customShortCode));
    }

    [Fact]
    public async Task GetOriginalUrlAsync_ExistingShortCode_ReturnsOriginalUrl()
    {
        var shortCode = "abc123";
        var originalUrl = "https://www.example.com";
        var shortenedUrl = new ShortenedUrl
        {
            Id = Guid.NewGuid(),
            ShortCode = shortCode,
            OriginalUrl = originalUrl,
            CreatedAt = DateTime.UtcNow,
            ClickCount = 0
        };

        _urlCacheServiceMock.Setup(c => c.GetAsync<ShortenedUrl>(It.IsAny<string>()))
            .ReturnsAsync((ShortenedUrl?)null);
        _urlRepositoryMock.Setup(r => r.GetByShortCodeAsync(shortCode))
            .ReturnsAsync(shortenedUrl);
        _urlRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ShortenedUrl>()))
            .ReturnsAsync(true);

        var result = await _urlService.GetOriginalUrlAsync(shortCode);

        // Assert
        Assert.Equal(originalUrl, result);
        _urlRepositoryMock.Verify(r => r.UpdateAsync(It.Is<ShortenedUrl>(u => u.ClickCount == 1)), Times.Once);
    }

    [Fact]
    public async Task GetOriginalUrlAsync_NonExistentShortCode_ThrowsNotFoundException()
    {
        var shortCode = "notfound";
        _urlCacheServiceMock.Setup(c => c.GetAsync<ShortenedUrl>(It.IsAny<string>()))
            .ReturnsAsync((ShortenedUrl?)null);
        _urlRepositoryMock.Setup(r => r.GetByShortCodeAsync(shortCode))
            .ReturnsAsync((ShortenedUrl?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UrlNotFoundException>(() =>
            _urlService.GetOriginalUrlAsync(shortCode));
    }

    [Fact]
    public async Task GetOriginalUrlAsync_ExpiredUrl_ThrowsUrlExpiredException()
    {
        var shortCode = "expired";
        var shortenedUrl = new ShortenedUrl
        {
            Id = Guid.NewGuid(),
            ShortCode = shortCode,
            OriginalUrl = "https://www.example.com",
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            ClickCount = 0
        };

        _urlCacheServiceMock.Setup(c => c.GetAsync<ShortenedUrl>(It.IsAny<string>()))
            .ReturnsAsync((ShortenedUrl?)null);
        _urlRepositoryMock.Setup(r => r.GetByShortCodeAsync(shortCode))
            .ReturnsAsync(shortenedUrl);

        // Act & Assert
        await Assert.ThrowsAsync<UrlExpiredException>(() =>
            _urlService.GetOriginalUrlAsync(shortCode));
    }

    [Fact]
    public async Task DeleteShortUrlAsync_ExistingUrl_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        var shortCode = "abc123";
        var url = new ShortenedUrl
        {
            Id = id,
            ShortCode = shortCode,
            OriginalUrl = "https://www.example.com",
            CreatedAt = DateTime.UtcNow
        };

        _urlRepositoryMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(url);
        _urlRepositoryMock.Setup(r => r.DeleteAsync(id))
            .ReturnsAsync(true);

        var result = await _urlService.DeleteShortUrlAsync(id);

        // Assert
        Assert.True(result);
        _urlCacheServiceMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
        _urlRepositoryMock.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    [Fact]
    public async Task DeleteShortUrlAsync_NonExistentUrl_ReturnsFalse()
    {
        var id = Guid.NewGuid();
        _urlRepositoryMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((ShortenedUrl?)null);

        var result = await _urlService.DeleteShortUrlAsync(id);

        // Assert
        Assert.False(result);
    }
}
