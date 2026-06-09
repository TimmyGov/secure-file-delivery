using FluentValidation;

namespace SecureFileDelivery.Application.Queries.RedeemDownloadToken;

public sealed class RedeemDownloadTokenQueryValidator : AbstractValidator<RedeemDownloadTokenQuery>
{
    public RedeemDownloadTokenQueryValidator()
    {
        RuleFor(x => x.RawToken).NotEmpty();
    }
}
