using FluentValidation;

namespace SecureFileDelivery.Application.Commands.UploadStatement;

public sealed class UploadStatementCommandValidator : AbstractValidator<UploadStatementCommand>
{
    public UploadStatementCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty();
        RuleFor(x => x.FileStream).NotNull();
        RuleFor(x => x.ContentType).Equal("application/pdf");
        RuleFor(x => x.FileSizeBytes).GreaterThan(0).LessThanOrEqualTo(52_428_800);
    }
}
