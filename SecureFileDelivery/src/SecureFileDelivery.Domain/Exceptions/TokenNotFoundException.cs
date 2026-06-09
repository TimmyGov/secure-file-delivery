namespace SecureFileDelivery.Domain.Exceptions;

public class TokenNotFoundException : Exception
{
    public TokenNotFoundException(string message = "The download token was not found.") : base(message)
    {
    }
}
