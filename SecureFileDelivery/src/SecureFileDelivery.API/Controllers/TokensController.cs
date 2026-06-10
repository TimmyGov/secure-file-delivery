using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureFileDelivery.Application.Commands.RevokeDownloadToken;

namespace SecureFileDelivery.API.Controllers;

[ApiController]
[Route("api/tokens")]
[Authorize]
public sealed class TokensController : ControllerBase
{
    private readonly IMediator _mediator;

    public TokensController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpDelete("{tokenId:guid}")]
    public async Task<IActionResult> Revoke(Guid tokenId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RevokeDownloadTokenCommand(tokenId, User.Identity?.Name ?? "api-user"), cancellationToken);
        return NoContent();
    }
}
