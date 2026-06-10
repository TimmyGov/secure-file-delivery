using MediatR;

namespace SecureFileDelivery.Application.Commands.DeleteStatement;

public sealed record DeleteStatementCommand(Guid StatementId, string DeletedBy) : IRequest<bool>;
