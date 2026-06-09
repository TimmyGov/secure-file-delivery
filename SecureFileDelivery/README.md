# Secure File Statement Delivery

Production-grade REST API for secure statement upload, tokenized download, audit logging, PostgreSQL persistence, and pluggable MinIO/local storage using .NET 8 Clean Architecture.

## Solution layout

- `SecureFileDelivery/src` - Domain, Application, Infrastructure, API
- `SecureFileDelivery/tests` - Domain, Application, Integration tests
- `Dockerfile` / `docker-compose.yml` - containerized runtime stack

## Quick start

```bash
cd SecureFileDelivery
dotnet build SecureFileDelivery.sln
dotnet test SecureFileDelivery.sln
```

API project configuration lives in `SecureFileDelivery/src/SecureFileDelivery.API/appsettings*.json`.
