using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using SecureFileDelivery.API.Contracts;
using SecureFileDelivery.Application.DTOs;

namespace SecureFileDelivery.Integration.Tests;

public sealed class ApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateJwt());
    }

    [Fact]
    public async Task UploadGenerateRedeem_ShouldSucceed()
    {
        var statement = await UploadStatementAsync();
        var token = await GenerateTokenAsync(statement.Id, false);

        var response = await _client.GetAsync($"/api/download/{token.RawToken}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
    }

    [Fact]
    public async Task RedeemingSingleUseTokenTwice_ShouldReturnGoneOnSecondAttempt()
    {
        var statement = await UploadStatementAsync();
        var token = await GenerateTokenAsync(statement.Id, false);

        var first = await _client.GetAsync($"/api/download/{token.RawToken}");
        var second = await _client.GetAsync($"/api/download/{token.RawToken}");

        first.StatusCode.Should().Be(HttpStatusCode.OK);
        second.StatusCode.Should().Be(HttpStatusCode.Gone);
    }

    [Fact]
    public async Task RevokedToken_ShouldReturnGoneWhenRedeemed()
    {
        var statement = await UploadStatementAsync();
        var token = await GenerateTokenAsync(statement.Id, false);

        var revokeResponse = await _client.DeleteAsync($"/api/tokens/{token.Id}");
        var redeemResponse = await _client.GetAsync($"/api/download/{token.RawToken}");

        revokeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        redeemResponse.StatusCode.Should().Be(HttpStatusCode.Gone);
    }

    [Fact]
    public async Task NonExistentToken_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync("/api/download/does-not-exist");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UnauthenticatedStatementUpload_ShouldReturnUnauthorized()
    {
        using var unauthenticatedClient = _factory.CreateClient();
        using var content = new MultipartFormDataContent
        {
            { new StringContent(Guid.NewGuid().ToString()), "customerId" },
            { new ByteArrayContent(Encoding.UTF8.GetBytes("%PDF-1.4")) { Headers = { ContentType = new MediaTypeHeaderValue("application/pdf") } }, "file", "statement.pdf" }
        };

        var response = await unauthenticatedClient.PostAsync("/api/statements", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UploadingNonPdf_ShouldReturnBadRequest()
    {
        using var content = new MultipartFormDataContent
        {
            { new StringContent(Guid.NewGuid().ToString()), "customerId" },
            { new ByteArrayContent(Encoding.UTF8.GetBytes("hello")) { Headers = { ContentType = new MediaTypeHeaderValue("text/plain") } }, "file", "statement.txt" }
        };

        var response = await _client.PostAsync("/api/statements", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<StatementDto> UploadStatementAsync()
    {
        using var content = new MultipartFormDataContent
        {
            { new StringContent(Guid.NewGuid().ToString()), "customerId" },
            { new ByteArrayContent(Encoding.UTF8.GetBytes("%PDF-1.4 sample")) { Headers = { ContentType = new MediaTypeHeaderValue("application/pdf") } }, "file", "statement.pdf" }
        };

        var response = await _client.PostAsync("/api/statements", content);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<StatementDto>(_jsonOptions))!;
    }

    private async Task<DownloadTokenDto> GenerateTokenAsync(Guid statementId, bool isMultiUse)
    {
        var response = await _client.PostAsJsonAsync($"/api/statements/{statementId}/tokens", new GenerateTokenRequest(60, isMultiUse));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<DownloadTokenDto>(_jsonOptions))!;
    }

    private static string CreateJwt()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("development-only-secret-change-me-32!!"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "SecureFileDelivery",
            audience: "SecureFileDelivery",
            claims: [new Claim(ClaimTypes.Name, "integration-user")],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
