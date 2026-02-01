namespace URLShortener.API.DTOs;

public class ShortUrlResponse
{
    public Guid Id { get; set; }
    public string ShortCode { get; set; } = string.Empty;
    public string OriginalUrl { get; set; } = string.Empty;
    public string ShortUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int ClickCount { get; set; }
}
