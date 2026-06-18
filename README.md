# Secure File Delivery

This workspace contains the Secure File Statement Delivery API.

Primary documentation lives in:

- [SecureFileDelivery/README.md](SecureFileDelivery/README.md)

Use that guide for architecture, API endpoints, configuration, Docker setup, testing, troubleshooting, and production notes.

## Quick start

```bash
cd SecureFileDelivery
dotnet restore SecureFileDelivery.sln
dotnet build SecureFileDelivery.sln
dotnet test SecureFileDelivery.sln
```

## Docker quick start

```bash
cd SecureFileDelivery
docker compose up -d --build
```

Create local env file first:

- PowerShell: `Copy-Item .env.example .env`
- Bash: `cp .env.example .env`

Health checks:

- http://localhost:8080/health/live
- http://localhost:8080/health/ready

## Contributor commands

Run unit tests only:

```bash
cd SecureFileDelivery
dotnet test tests/SecureFileDelivery.Domain.Tests/SecureFileDelivery.Domain.Tests.csproj --configuration Release
dotnet test tests/SecureFileDelivery.Application.Tests/SecureFileDelivery.Application.Tests.csproj --configuration Release
```

Run integration tests:

```bash
cd SecureFileDelivery
dotnet test tests/SecureFileDelivery.Integration.Tests/SecureFileDelivery.Integration.Tests.csproj --configuration Release
```

View compose service status:

```bash
cd SecureFileDelivery
docker compose ps
```

View API logs:

```bash
cd SecureFileDelivery
docker compose logs api --tail 100
```

Stop stack:

```bash
cd SecureFileDelivery
docker compose down
```

Stop stack and remove volumes:

```bash
cd SecureFileDelivery
docker compose down -v
```
