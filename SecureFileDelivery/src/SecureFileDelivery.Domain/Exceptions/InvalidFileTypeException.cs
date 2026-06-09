namespace SecureFileDelivery.Domain.Exceptions;

public class InvalidFileTypeException : Exception
{
    public InvalidFileTypeException(string message = "Only PDF files are supported.") : base(message)
    {
    }
}
