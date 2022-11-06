namespace SS14.Issues.Exceptions;

public class RateLimitException : Exception
{
    public RateLimitException(string? message) : base(message)
    {
    }
}