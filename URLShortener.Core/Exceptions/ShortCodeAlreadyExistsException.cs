namespace URLShortener.Core.Exceptions;

public class ShortCodeAlreadyExistsException : UrlShortenerException
{
    public ShortCodeAlreadyExistsException(string shortCode)
        : base($"The short code '{shortCode}' already exists.")
    {
    }
}
