namespace SecureFileDelivery.Domain.Interfaces;

public interface ITokenHasher
{
    string Hash(string rawToken);
    bool Verify(string rawToken, string hash);
}
