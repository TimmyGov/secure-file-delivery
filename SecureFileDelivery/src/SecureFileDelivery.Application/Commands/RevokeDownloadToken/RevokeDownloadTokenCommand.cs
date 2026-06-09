using MediatR;

namespace SecureFileDelivery.Application.Commands.RevokeDownloadToken;

public sealed record RevokeDownloadTokenCommand(Guid TokenId, string RevokedBy) : IRequest<bool>;
