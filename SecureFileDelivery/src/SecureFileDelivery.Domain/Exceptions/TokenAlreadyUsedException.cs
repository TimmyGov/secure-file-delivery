namespace SecureFileDelivery.Domain.Exceptions;

public class TokenAlreadyUsedException : Exception
{
    public TokenAlreadyUsedException(string message = "The download token has already been used.") : base(message)
    {
    }
}
