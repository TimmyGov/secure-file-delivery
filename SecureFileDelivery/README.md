# Secure File Statement Delivery

Secure File Statement Delivery is a .NET 8 REST API for secure statement upload and tokenized download.
It follows a Clean Architecture layout and includes:

- JWT-based authentication
- Statement upload, retrieval, delete, and audit history
- One-time or multi-use download tokens
- PostgreSQL persistence
- Pluggable file storage (MinIO or local filesystem)
- Structured logging, readiness/liveness health checks, and rate limiting

## Solution layout

- `src/SecureFileDelivery.Domain` - domain entities, value objects, interfaces, and domain exceptions
- `src/SecureFileDelivery.Application` - command/query handlers, DTOs, validation, mapping, behavior pipeline
- `src/SecureFileDelivery.Infrastructure` - EF Core persistence, repositories, storage clients, background services
- `src/SecureFileDelivery.API` - HTTP controllers, middleware, authentication, health checks, startup wiring
- `tests/SecureFileDelivery.Domain.Tests` - domain unit tests
- `tests/SecureFileDelivery.Application.Tests` - application unit tests
- `tests/SecureFileDelivery.Integration.Tests` - end-to-end API integration tests

## Core capabilities

- Upload PDF statements with validation and audit logging
- List statements by customer with pagination
- Get statement by id
- Soft-delete statements
- Generate download tokens with configurable TTL and single/multi-use behavior
- Redeem download tokens via anonymous endpoint
- Revoke issued tokens
- Query audit trail for statement activity

## API endpoints

Base route: `/api`

- `POST /api/statements` (authorized)
	- Multipart form data: `customerId`, `file`
	- Accepts PDF only
	- Rate limit policy: `statement-upload`
- `GET /api/statements/customer/{customerId}` (authorized)
- `GET /api/statements/{id}` (authorized)
- `DELETE /api/statements/{id}` (authorized)
- `POST /api/statements/{id}/tokens` (authorized)
	- JSON body: `{ "ttlMinutes": 60, "isMultiUse": false }`
- `GET /api/statements/{id}/audit` (authorized)
- `DELETE /api/tokens/{tokenId}` (authorized)
- `GET /api/download/{token}` (anonymous)
	- Rate limit policy: `download`

Health endpoints:

- `GET /health/live` - liveness only
- `GET /health/ready` - readiness (database + storage)

## Security and operational behavior

- JWT issuer, audience, and signature validation enabled
- Production startup validation rejects missing or weak secrets
- Global exception middleware returns ProblemDetails and includes `traceId`
- Internal exception details are hidden for 500 responses
- Security headers middleware adds common hardening headers
- Request rate limiting is applied to upload and download endpoints
- Database migrations are applied on startup
- MinIO bucket is ensured at startup when MinIO provider is enabled

## Configuration

Main configuration files:

- `src/SecureFileDelivery.API/appsettings.json` - production-safe defaults and empty secret placeholders
- `src/SecureFileDelivery.API/appsettings.Development.json` - local development defaults

Important runtime settings:

- `ConnectionStrings__DefaultConnection`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Jwt__SecretKey` (minimum 32 chars)
- `Storage__Provider` (`Minio` or `Local`)
- `Storage__Minio__Endpoint`
- `Storage__Minio__AccessKey`
- `Storage__Minio__SecretKey`
- `Storage__Minio__BucketName`
- `Storage__Minio__UseSSL`

## Local development (without Docker)

Prerequisites:

- .NET SDK 8.x
- PostgreSQL (if using Npgsql)
- Optional: MinIO for object storage

Restore, build, test:

```bash
dotnet restore SecureFileDelivery.sln
dotnet build SecureFileDelivery.sln
dotnet test SecureFileDelivery.sln
```

Run API project:

```bash
dotnet run --project src/SecureFileDelivery.API/SecureFileDelivery.API.csproj
```

Default local launch profile serves on:

- `http://localhost:5169`
- `https://localhost:7192`

Swagger UI is enabled in Development only.

## Docker

Docker assets:

- `Dockerfile`
- `docker-compose.yml`
- `docker-compose.override.yml`
- `.env.example` (template)

The `.env` file is intentionally untracked in git.

Setup steps:

1. Copy `.env.example` to `.env`.
2. Replace placeholder values in `.env` with real secrets.
3. Start the stack:

```bash
docker compose up -d --build
```

4. Verify health:

```bash
curl http://localhost:8080/health/live
curl http://localhost:8080/health/ready
```

Services started by compose:

- API on port `8080`
- PostgreSQL 16 (internal network)
- MinIO API on `9000`
- MinIO console on `9001`

The override file applies production-oriented runtime defaults:

- `restart: unless-stopped`
- `init: true`
- capped json-file logging per service

## Running tests

Run all tests:

```bash
dotnet test SecureFileDelivery.sln --configuration Release
```

Run only unit tests:

```bash
dotnet test tests/SecureFileDelivery.Domain.Tests/SecureFileDelivery.Domain.Tests.csproj --configuration Release
dotnet test tests/SecureFileDelivery.Application.Tests/SecureFileDelivery.Application.Tests.csproj --configuration Release
```

Run integration tests:

```bash
dotnet test tests/SecureFileDelivery.Integration.Tests/SecureFileDelivery.Integration.Tests.csproj --configuration Release
```

## EF Core tooling

This repository includes a local tool manifest with `dotnet-ef`.

```bash
dotnet tool restore
dotnet ef --help
```

## Troubleshooting

- Readiness returns 503:
	- Check API logs and verify database/storage connectivity.
	- Confirm MinIO credentials and bucket settings are correct.
- Startup fails with production configuration error:
	- One or more required settings are missing or still placeholder values.
- Token download issues:
	- Verify token TTL, revoked/used state, and clock sync.

## Production notes

Current setup is deployment-capable, but production environments should additionally ensure:

- Secrets are provided via secret manager or deployment platform, not committed files
- TLS termination and secure ingress are configured
- Centralized log and trace sink is configured (beyond console/file defaults)
- Backup, restore, and rollback procedures are documented and tested
- CI/CD includes dependency, container, and secret scanning
