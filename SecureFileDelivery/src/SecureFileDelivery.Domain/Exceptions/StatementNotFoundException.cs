namespace SecureFileDelivery.Domain.Exceptions;

public class StatementNotFoundException : Exception
{
    public StatementNotFoundException(string message = "The statement was not found.") : base(message)
    {
    }
}
