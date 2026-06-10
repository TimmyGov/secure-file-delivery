using FluentAssertions;
using Moq;
using SecureFileDelivery.Application.Commands.GenerateDownloadToken;
using SecureFileDelivery.Application.Commands.RevokeDownloadToken;
using SecureFileDelivery.Application.Commands.UploadStatement;
using SecureFileDelivery.Application.Queries.RedeemDownloadToken;
using SecureFileDelivery.Domain.Entities;
using SecureFileDelivery.Domain.Exceptions;
using SecureFileDelivery.Domain.Interfaces;
using SecureFileDelivery.Domain.ValueObjects;

namespace SecureFileDelivery.Application.Tests;

public sealed class HandlerTests
{
    private readonly DateTime _now = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task UploadStatementHandler_ShouldUploadStatement()
    {
        var statementRepository = new Mock<IStatementRepository>();
        var auditRepository = new Mock<IAuditLogRepository>();
        var storage = new Mock<IFileStorage>();
        storage.Setup(x => x.UploadAsync(It.IsAny<Stream>(), "statement.pdf", "application/pdf")).ReturnsAsync("stored.pdf");
        var handler = new UploadStatementCommandHandler(statementRepository.Object, auditRepository.Object, storage.Object, new TestDateTimeProvider(_now));

        var result = await handler.Handle(new UploadStatementCommand(Guid.NewGuid(), "statement.pdf", new MemoryStream(new byte[] { 1, 2, 3 }), "application/pdf", 3), CancellationToken.None);

        result.FileName.Should().Be("statement.pdf");
        result.ContentType.Should().Be("application/pdf");
        statementRepository.Verify(x => x.AddAsync(It.IsAny<Statement>()), Times.Once);
        auditRepository.Verify(x => x.AddAsync(It.IsAny<AuditLog>()), Times.Once);
    }

    [Fact]
    public async Task UploadStatementHandler_ShouldThrowForInvalidContentType()
    {
        var handler = new UploadStatementCommandHandler(
            Mock.Of<IStatementRepository>(),
            Mock.Of<IAuditLogRepository>(),
            Mock.Of<IFileStorage>(),
            new TestDateTimeProvider(_now));

        var action = () => handler.Handle(new UploadStatementCommand(Guid.NewGuid(), "statement.txt", new MemoryStream([1]), "text/plain", 1), CancellationToken.None);

        await action.Should().ThrowAsync<InvalidFileTypeException>();
    }

    [Fact]
    public async Task GenerateDownloadTokenHandler_ShouldGenerateToken()
    {
        var statementRepository = new Mock<IStatementRepository>();
        var tokenRepository = new Mock<IDownloadTokenRepository>();
        var auditRepository = new Mock<IAuditLogRepository>();
        var tokenGenerator = new Mock<ITokenGenerator>();
        var tokenHasher = new Mock<ITokenHasher>();
        var statementId = Guid.NewGuid();
        statementRepository.Setup(x => x.GetByIdAsync(new StatementId(statementId)))
            .ReturnsAsync(new Statement(statementId, new CustomerId(Guid.NewGuid()), "statement.pdf", "path", 10, "application/pdf", _now));
        tokenGenerator.Setup(x => x.Generate()).Returns("raw-token");
        tokenHasher.Setup(x => x.Hash("raw-token")).Returns("hashed-token");
        var handler = new GenerateDownloadTokenCommandHandler(statementRepository.Object, tokenRepository.Object, auditRepository.Object, tokenGenerator.Object, tokenHasher.Object, new TestDateTimeProvider(_now));

        var result = await handler.Handle(new GenerateDownloadTokenCommand(statementId, "tester", 60, false), CancellationToken.None);

        result.RawToken.Should().Be("raw-token");
        tokenRepository.Verify(x => x.AddAsync(It.IsAny<DownloadToken>()), Times.Once);
        auditRepository.Verify(x => x.AddAsync(It.IsAny<AuditLog>()), Times.Once);
    }

    [Fact]
    public async Task GenerateDownloadTokenHandler_ShouldThrowWhenStatementMissing()
    {
        var statementRepository = new Mock<IStatementRepository>();
        statementRepository.Setup(x => x.GetByIdAsync(It.IsAny<StatementId>())).ReturnsAsync((Statement?)null);
        var handler = new GenerateDownloadTokenCommandHandler(statementRepository.Object, Mock.Of<IDownloadTokenRepository>(), Mock.Of<IAuditLogRepository>(), Mock.Of<ITokenGenerator>(), Mock.Of<ITokenHasher>(), new TestDateTimeProvider(_now));

        var action = () => handler.Handle(new GenerateDownloadTokenCommand(Guid.NewGuid(), "tester"), CancellationToken.None);

        await action.Should().ThrowAsync<StatementNotFoundException>();
    }

    [Fact]
    public async Task RedeemDownloadTokenHandler_ShouldThrowWhenTokenMissing()
    {
        var tokenRepository = new Mock<IDownloadTokenRepository>();
        tokenRepository.Setup(x => x.GetByTokenHashAsync("hash")).ReturnsAsync((DownloadToken?)null);
        var hasher = new Mock<ITokenHasher>();
        hasher.Setup(x => x.Hash("raw")).Returns("hash");
        var handler = CreateRedeemHandler(tokenRepository: tokenRepository.Object, tokenHasher: hasher.Object);

        var action = () => handler.Handle(new RedeemDownloadTokenQuery("raw", "127.0.0.1", "tester"), CancellationToken.None);

        await action.Should().ThrowAsync<TokenNotFoundException>();
    }

    [Fact]
    public async Task RedeemDownloadTokenHandler_ShouldThrowWhenExpired()
    {
        var token = new DownloadToken(Guid.NewGuid(), new StatementId(Guid.NewGuid()), "hash", DateTime.UtcNow.AddMinutes(-1), _now, false);
        var handler = CreateRedeemHandler(token: token);

        var action = () => handler.Handle(new RedeemDownloadTokenQuery("raw", "127.0.0.1", "tester"), CancellationToken.None);

        await action.Should().ThrowAsync<TokenExpiredException>();
    }

    [Fact]
    public async Task RedeemDownloadTokenHandler_ShouldThrowWhenRevoked()
    {
        var token = new DownloadToken(Guid.NewGuid(), new StatementId(Guid.NewGuid()), "hash", DateTime.UtcNow.AddMinutes(5), _now, false);
        token.Revoke();
        var handler = CreateRedeemHandler(token: token);

        var action = () => handler.Handle(new RedeemDownloadTokenQuery("raw", "127.0.0.1", "tester"), CancellationToken.None);

        await action.Should().ThrowAsync<TokenRevokedException>();
    }

    [Fact]
    public async Task RedeemDownloadTokenHandler_ShouldThrowWhenAlreadyUsed()
    {
        var token = new DownloadToken(Guid.NewGuid(), new StatementId(Guid.NewGuid()), "hash", DateTime.UtcNow.AddMinutes(5), _now, false);
        token.MarkAsUsed();
        var handler = CreateRedeemHandler(token: token);

        var action = () => handler.Handle(new RedeemDownloadTokenQuery("raw", "127.0.0.1", "tester"), CancellationToken.None);

        await action.Should().ThrowAsync<TokenAlreadyUsedException>();
    }

    [Fact]
    public async Task RedeemDownloadTokenHandler_ShouldRedeemSingleUseToken()
    {
        var token = new DownloadToken(Guid.NewGuid(), new StatementId(Guid.NewGuid()), "hash", DateTime.UtcNow.AddMinutes(5), _now, false);
        var tokenRepository = new Mock<IDownloadTokenRepository>();
        tokenRepository.Setup(x => x.GetByTokenHashAsync("hash")).ReturnsAsync(token);
        var statementRepository = new Mock<IStatementRepository>();
        statementRepository.Setup(x => x.GetByIdAsync(token.StatementId)).ReturnsAsync(new Statement(token.StatementId.Value, new CustomerId(Guid.NewGuid()), "statement.pdf", "path", 10, "application/pdf", _now));
        var fileStorage = new Mock<IFileStorage>();
        fileStorage.Setup(x => x.DownloadAsync("path")).ReturnsAsync(new MemoryStream([1, 2, 3]));
        var hasher = new Mock<ITokenHasher>();
        hasher.Setup(x => x.Hash("raw")).Returns("hash");
        var handler = new RedeemDownloadTokenQueryHandler(tokenRepository.Object, statementRepository.Object, Mock.Of<IAuditLogRepository>(), hasher.Object, fileStorage.Object, new TestDateTimeProvider(_now));

        var result = await handler.Handle(new RedeemDownloadTokenQuery("raw", "127.0.0.1", "tester"), CancellationToken.None);

        result.FileName.Should().Be("statement.pdf");
        token.UsedAt.Should().NotBeNull();
        tokenRepository.Verify(x => x.UpdateAsync(token), Times.Once);
    }

    [Fact]
    public async Task RedeemDownloadTokenHandler_ShouldRedeemMultiUseTokenWithoutUpdating()
    {
        var token = new DownloadToken(Guid.NewGuid(), new StatementId(Guid.NewGuid()), "hash", DateTime.UtcNow.AddMinutes(5), _now, true);
        token.MarkAsUsed();
        var tokenRepository = new Mock<IDownloadTokenRepository>();
        tokenRepository.Setup(x => x.GetByTokenHashAsync("hash")).ReturnsAsync(token);
        var statementRepository = new Mock<IStatementRepository>();
        statementRepository.Setup(x => x.GetByIdAsync(token.StatementId)).ReturnsAsync(new Statement(token.StatementId.Value, new CustomerId(Guid.NewGuid()), "statement.pdf", "path", 10, "application/pdf", _now));
        var fileStorage = new Mock<IFileStorage>();
        fileStorage.Setup(x => x.DownloadAsync("path")).ReturnsAsync(new MemoryStream([1, 2, 3]));
        var hasher = new Mock<ITokenHasher>();
        hasher.Setup(x => x.Hash("raw")).Returns("hash");
        var handler = new RedeemDownloadTokenQueryHandler(tokenRepository.Object, statementRepository.Object, Mock.Of<IAuditLogRepository>(), hasher.Object, fileStorage.Object, new TestDateTimeProvider(_now));

        var result = await handler.Handle(new RedeemDownloadTokenQuery("raw", "127.0.0.1", "tester"), CancellationToken.None);

        result.ContentType.Should().Be("application/pdf");
        tokenRepository.Verify(x => x.UpdateAsync(It.IsAny<DownloadToken>()), Times.Never);
    }

    [Fact]
    public async Task RevokeDownloadTokenHandler_ShouldRevokeToken()
    {
        var token = new DownloadToken(Guid.NewGuid(), new StatementId(Guid.NewGuid()), "hash", DateTime.UtcNow.AddMinutes(5), _now, false);
        var tokenRepository = new Mock<IDownloadTokenRepository>();
        tokenRepository.Setup(x => x.GetByIdAsync(new TokenId(token.Id))).ReturnsAsync(token);
        var handler = new RevokeDownloadTokenCommandHandler(tokenRepository.Object, Mock.Of<IAuditLogRepository>(), new TestDateTimeProvider(_now));

        var result = await handler.Handle(new RevokeDownloadTokenCommand(token.Id, "tester"), CancellationToken.None);

        result.Should().BeTrue();
        token.IsRevoked.Should().BeTrue();
        tokenRepository.Verify(x => x.UpdateAsync(token), Times.Once);
    }

    [Fact]
    public async Task RevokeDownloadTokenHandler_ShouldThrowWhenTokenMissing()
    {
        var tokenRepository = new Mock<IDownloadTokenRepository>();
        tokenRepository.Setup(x => x.GetByIdAsync(It.IsAny<TokenId>())).ReturnsAsync((DownloadToken?)null);
        var handler = new RevokeDownloadTokenCommandHandler(tokenRepository.Object, Mock.Of<IAuditLogRepository>(), new TestDateTimeProvider(_now));

        var action = () => handler.Handle(new RevokeDownloadTokenCommand(Guid.NewGuid(), "tester"), CancellationToken.None);

        await action.Should().ThrowAsync<TokenNotFoundException>();
    }

    private RedeemDownloadTokenQueryHandler CreateRedeemHandler(DownloadToken? token = null, IDownloadTokenRepository? tokenRepository = null, ITokenHasher? tokenHasher = null)
    {
        var tokenRepositoryMock = tokenRepository is null ? new Mock<IDownloadTokenRepository>() : Mock.Get(tokenRepository);
        var tokenHasherMock = tokenHasher is null ? new Mock<ITokenHasher>() : Mock.Get(tokenHasher);
        var statementRepository = Mock.Of<IStatementRepository>();
        var auditRepository = Mock.Of<IAuditLogRepository>();
        var fileStorage = Mock.Of<IFileStorage>();

        if (token is not null)
        {
            tokenRepositoryMock.Setup(x => x.GetByTokenHashAsync("hash")).ReturnsAsync(token);
        }

        tokenHasherMock.Setup(x => x.Hash("raw")).Returns("hash");

        return new RedeemDownloadTokenQueryHandler(tokenRepositoryMock.Object, statementRepository, auditRepository, tokenHasherMock.Object, fileStorage, new TestDateTimeProvider(_now));
    }
}
