namespace URLShortener.Core.Exceptions;

public class InvalidUrlException : UrlShortenerException
{
    public InvalidUrlException(string url)
        : base($"The URL '{url}' is not valid.")
    {
    }
}
