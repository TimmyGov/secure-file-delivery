using FluentAssertions;
using FluentValidation;
using SecureFileDelivery.Application.Commands.UploadStatement;
using SecureFileDelivery.Application.Common.Behaviours;

namespace SecureFileDelivery.Application.Tests;

public sealed class ValidationBehaviourTests
{
    [Fact]
    public async Task ValidationBehaviour_ShouldThrowForInvalidRequest()
    {
        var validators = new IValidator<UploadStatementCommand>[] { new UploadStatementCommandValidator() };
        var behaviour = new ValidationBehaviour<UploadStatementCommand, string>(validators);

        var action = () => behaviour.Handle(
            new UploadStatementCommand(Guid.Empty, string.Empty, null!, "text/plain", 0),
            _ => Task.FromResult("ok"),
            CancellationToken.None);

        await action.Should().ThrowAsync<ValidationException>();
    }
}
