namespace SecureFileDelivery.Domain.ValueObjects;

public readonly record struct StatementId(Guid Value)
{
    public static StatementId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
