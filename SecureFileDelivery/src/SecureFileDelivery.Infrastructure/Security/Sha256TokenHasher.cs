using System.Security.Cryptography;
using System.Text;
using SecureFileDelivery.Domain.Interfaces;

namespace SecureFileDelivery.Infrastructure.Security;

public sealed class Sha256TokenHasher : ITokenHasher
{
    public string Hash(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public bool Verify(string rawToken, string hash) => string.Equals(Hash(rawToken), hash, StringComparison.Ordinal);
}
