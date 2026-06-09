using System.Security.Cryptography;
using SecureFileDelivery.Domain.Interfaces;

namespace SecureFileDelivery.Infrastructure.Security;

public sealed class TokenGenerator : ITokenGenerator
{
    public string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", string.Empty, StringComparison.Ordinal);
    }
}
