using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SecureFileDelivery.Application.Queries.RedeemDownloadToken;

namespace SecureFileDelivery.API.Controllers;

[ApiController]
[Route("api/download")]
public sealed class DownloadController : ControllerBase
{
    private readonly IMediator _mediator;

    public DownloadController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{token}")]
    [AllowAnonymous]
    [EnableRateLimiting("download")]
    public async Task<IActionResult> Download(string token, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RedeemDownloadTokenQuery(
            token,
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            User.Identity?.Name ?? "anonymous"), cancellationToken);

        return File(result.Stream, result.ContentType, result.FileName);
    }
}
