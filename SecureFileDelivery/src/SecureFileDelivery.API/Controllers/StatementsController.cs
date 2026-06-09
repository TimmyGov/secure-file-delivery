using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SecureFileDelivery.API.Contracts;
using SecureFileDelivery.Application.Commands.DeleteStatement;
using SecureFileDelivery.Application.Commands.GenerateDownloadToken;
using SecureFileDelivery.Application.Commands.UploadStatement;
using SecureFileDelivery.Application.Queries.GetAuditLogsForStatement;
using SecureFileDelivery.Application.Queries.GetStatementById;
using SecureFileDelivery.Application.Queries.GetStatementsByCustomer;

namespace SecureFileDelivery.API.Controllers;

[ApiController]
[Route("api/statements")]
[Authorize]
public sealed class StatementsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StatementsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [EnableRateLimiting("statement-upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult> UploadStatement([FromForm] Guid customerId, [FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (customerId == Guid.Empty)
        {
            ModelState.AddModelError(nameof(customerId), "CustomerId is required.");
        }

        if (file is null || file.Length == 0)
        {
            ModelState.AddModelError(nameof(file), "A PDF file is required.");
        }
        else if (!string.Equals(file.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(file), "Only PDF files are supported.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var validatedFile = file!;
        await using var stream = validatedFile.OpenReadStream();
        var result = await _mediator.Send(new UploadStatementCommand(customerId, validatedFile.FileName, stream, validatedFile.ContentType, validatedFile.Length), cancellationToken);
        return CreatedAtAction(nameof(GetStatementById), new { id = result.Id }, result);
    }

    [HttpGet("customer/{customerId:guid}")]
    public async Task<ActionResult> GetStatementsByCustomer(Guid customerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetStatementsByCustomerQuery(customerId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetStatementById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStatementByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteStatement(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteStatementCommand(id, User.Identity?.Name ?? "api-user"), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/tokens")]
    public async Task<ActionResult> GenerateToken(Guid id, [FromBody] GenerateTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GenerateDownloadTokenCommand(id, User.Identity?.Name ?? "api-user", request.TtlMinutes, request.IsMultiUse), cancellationToken);
        return Created($"/api/download/{result.RawToken}", result);
    }

    [HttpGet("{id:guid}/audit")]
    public async Task<ActionResult> GetAudit(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAuditLogsForStatementQuery(id, page, pageSize), cancellationToken);
        return Ok(result);
    }
}
