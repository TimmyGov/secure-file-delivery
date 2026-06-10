namespace SecureFileDelivery.Domain.Exceptions;

public class TokenExpiredException : Exception
{
    public TokenExpiredException(string message = "The download token has expired.") : base(message)
    {
    }
}
