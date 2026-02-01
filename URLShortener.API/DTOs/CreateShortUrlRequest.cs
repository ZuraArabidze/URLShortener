namespace URLShortener.API.DTOs;

public class CreateShortUrlRequest
{
    public string OriginalUrl { get; set; } = string.Empty;
    public string? CustomShortCode { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
