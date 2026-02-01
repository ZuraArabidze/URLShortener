namespace URLShortener.Core.Exceptions;

public class UrlShortenerException : Exception
{
    public UrlShortenerException(string message)
        : base(message)
    {
    }

    public UrlShortenerException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
