namespace URLShortener.Core.Exceptions;

public class UrlNotFoundException : UrlShortenerException
{
    public UrlNotFoundException (string shortCode)
        : base($"The URL with short code '{shortCode}' was not found.")
    {
    }
}
