using MediatR;
using SecureFileDelivery.Application.DTOs;

namespace SecureFileDelivery.Application.Queries.RedeemDownloadToken;

public sealed record RedeemDownloadTokenQuery(string RawToken, string IpAddress, string RedeemedBy) : IRequest<StatementFileResult>;
