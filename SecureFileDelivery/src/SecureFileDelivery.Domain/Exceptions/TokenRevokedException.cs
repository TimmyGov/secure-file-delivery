namespace SecureFileDelivery.Domain.Exceptions;

public class TokenRevokedException : Exception
{
    public TokenRevokedException(string message = "The download token has been revoked.") : base(message)
    {
    }
}
