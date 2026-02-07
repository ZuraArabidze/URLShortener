using Microsoft.EntityFrameworkCore;
using URLShortener.Core.Entities;
using URLShortener.Infrastructure.Data;
using URLShortener.Infrastructure.Repositories;

namespace URLShortener.Tests.Repositories;

public class UrlRepositoryTests
{
    private UrlShortenerDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<UrlShortenerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new UrlShortenerDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_ValidUrl_CreateSuccessfully()
    {
        using var context = GetInMemoryDbContext();
        var repository = new UrlRepository(context);
        var url = new ShortenedUrl
        {
            Id = Guid.NewGuid(),
            ShortCode = "test123",
            OriginalUrl = "https://www.example.com",
            CreatedAt = DateTime.UtcNow,
            ClickCount = 0,
            CreatedByIp = ""

        };

        var result = await repository.CreateAsync(url);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(url.Id, result.Id);
        Assert.Equal(url.ShortCode, result.ShortCode);
        Assert.Equal(1, await context.ShortenedUrls.CountAsync());
    }

    [Fact]
    public async Task GetByShortCodeAsync_ExistingCode_ReturnsUrl()
    {
        using var context = GetInMemoryDbContext();
        var repository = new UrlRepository(context);
        var url = new ShortenedUrl
        {
            Id = Guid.NewGuid(),
            ShortCode = "test123",
            OriginalUrl = "https://www.example.com",
            CreatedAt = DateTime.UtcNow,
            ClickCount = 0,
            CreatedByIp = ""
        };
        
        await context.ShortenedUrls.AddAsync(url);
        await context.SaveChangesAsync();

        var result = await repository.GetByShortCodeAsync("test123");

        //Assert
        Assert.NotNull(result);
        Assert.Equal(url.ShortCode, result.ShortCode);
        Assert.Equal(url.OriginalUrl, result.OriginalUrl);
    }

    [Fact]
    public async Task GetByShortCodeAsync_NonExistentCode_ReturnsNull()
    {
        using var context = GetInMemoryDbContext();
        var repository = new UrlRepository(context);

        var result = await repository.GetByShortCodeAsync("notfound");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ExistingUrl_UpdatesSuccessfully()
    {
        using var context = GetInMemoryDbContext();
        var repository = new UrlRepository(context);
        var url = new ShortenedUrl
        {
            Id = Guid.NewGuid(),
            ShortCode = "update",
            OriginalUrl = "https://www.example.com",
            CreatedAt = DateTime.UtcNow,
            ClickCount = 0
        };
        await context.ShortenedUrls.AddAsync(url);
        await context.SaveChangesAsync();

        url.ClickCount = 5;
        var result = await repository.UpdateAsync(url);

        // Assert
        Assert.True(result);
        var updated = await context.ShortenedUrls.FindAsync(url.Id);
        Assert.Equal(5, updated?.ClickCount);
    }

    [Fact]
    public async Task DeleteAsync_ExistingUrl_DeletesSuccessfully()
    {
        using var context = GetInMemoryDbContext();
        var repository = new UrlRepository(context);
        var url = new ShortenedUrl
        {
            Id = Guid.NewGuid(),
            ShortCode = "delete",
            OriginalUrl = "https://www.example.com",
            CreatedAt = DateTime.UtcNow,
            ClickCount = 0
        };
        await context.ShortenedUrls.AddAsync(url);
        await context.SaveChangesAsync();

        var result = await repository.DeleteAsync(url.Id);

        // Assert
        Assert.True(result);
        Assert.Equal(0, await context.ShortenedUrls.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_NonExistentUrl_ReturnsFalse()
    {
        using var context = GetInMemoryDbContext();
        var repository = new UrlRepository(context);

        var result = await repository.DeleteAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ShortCodeExistsAsync_ExistingCode_ReturnsTrue()
    {
        using var context = GetInMemoryDbContext();
        var repository = new UrlRepository(context);
        var url = new ShortenedUrl
        {
            Id = Guid.NewGuid(),
            ShortCode = "exists",
            OriginalUrl = "https://www.example.com",
            CreatedAt = DateTime.UtcNow,
            ClickCount = 0
        };
        await context.ShortenedUrls.AddAsync(url);
        await context.SaveChangesAsync();

        var result = await repository.ShortCodeExistsAsync("exists");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ShortCodeExistsAsync_NonExistentCode_ReturnsFalse()
    {
        using var context = GetInMemoryDbContext();
        var repository = new UrlRepository(context);

        var result = await repository.ShortCodeExistsAsync("notexists");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_ReturnsCorrectPage()
    {
        using var context = GetInMemoryDbContext();
        var repository = new UrlRepository(context);

        for (int i = 0; i < 15; i++)
        {
            await context.ShortenedUrls.AddAsync(new ShortenedUrl
            {
                Id = Guid.NewGuid(),
                ShortCode = $"code{i}",
                OriginalUrl = $"https://www.example{i}.com",
                CreatedAt = DateTime.UtcNow.AddMinutes(-i),
                ClickCount = 0
            });
        }
        await context.SaveChangesAsync();

        var page1 = await repository.GetAllAsync(1, 10);
        var page2 = await repository.GetAllAsync(2, 10);

        // Assert
        Assert.Equal(10, page1.Count());
        Assert.Equal(5, page2.Count());
    }
}
