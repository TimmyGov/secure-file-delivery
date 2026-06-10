namespace SecureFileDelivery.Domain.ValueObjects;

public readonly record struct TokenId(Guid Value)
{
    public static TokenId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
