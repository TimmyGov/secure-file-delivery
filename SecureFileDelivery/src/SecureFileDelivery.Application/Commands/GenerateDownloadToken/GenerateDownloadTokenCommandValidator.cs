using FluentValidation;

namespace SecureFileDelivery.Application.Commands.GenerateDownloadToken;

public sealed class GenerateDownloadTokenCommandValidator : AbstractValidator<GenerateDownloadTokenCommand>
{
    public GenerateDownloadTokenCommandValidator()
    {
        RuleFor(x => x.StatementId).NotEmpty();
        RuleFor(x => x.RequestedBy).NotEmpty();
        RuleFor(x => x.TtlMinutes).InclusiveBetween(1, 10_080);
    }
}
