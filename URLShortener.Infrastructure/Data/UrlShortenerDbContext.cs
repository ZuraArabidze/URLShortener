using Microsoft.EntityFrameworkCore;
using URLShortener.Core.Entities;

namespace URLShortener.Infrastructure.Data;

public class UrlShortenerDbContext : DbContext
{
    public UrlShortenerDbContext(DbContextOptions<UrlShortenerDbContext> options) 
        : base(options)
    {

    }

    public DbSet<ShortenedUrl> ShortenedUrls { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ShortenedUrl>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.ShortCode).IsUnique();
            entity.Property(e => e.ShortCode).IsRequired().HasMaxLength(10);

            entity.Property(e => e.OriginalUrl).IsRequired().HasMaxLength(2048);

            entity.Property(e => e.CreatedAt).IsRequired();

            entity.Property(e => e.ClickCount).HasDefaultValue(0);

            entity.Property(e => e.CreatedByIp).HasMaxLength(45);

        });
    }
}
