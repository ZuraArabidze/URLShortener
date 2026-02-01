namespace URLShortener.Core.Exceptions;

public class UrlExpiredException : UrlShortenerException
{
    public UrlExpiredException(string shortcode) 
        : base($"The URL with short code '{shortcode}' has expired.")
    {

    }
}
