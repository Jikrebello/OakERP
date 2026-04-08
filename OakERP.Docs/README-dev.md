# OakERP Developer Guide

This is the main onboarding entrypoint for working on OakERP locally.

## First-Run Setup

1. Start the local database stack:
   - [db-setup.md](db-setup.md)
2. Create local host overrides:
   - `OakERP.API/appsettings.Local.json`
   - `OakERP.Web/appsettings.Local.json`
3. Trust the local HTTPS certificate:
   - [local-https-dev-cert.md](local-https-dev-cert.md)
4. Apply or inspect schema changes when needed:
   - [ef-core.md](ef-core.md)
5. Run the test suites you need:
   - [oakERP-testing-guide.md](oakERP-testing-guide.md)

## Local Host Defaults

### API

- HTTPS: `https://localhost:7057`
- HTTP fallback: `http://localhost:5169`
- Swagger: `/swagger`

Typical local API override file:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5433;Database=oakerp;Username=oakadmin;Password=oakpass;Include Error Detail=true"
  },
  "JwtSettings": {
    "Key": "ThisIsALocalDevJwtKeyThatIsLongEnough123!",
    "Issuer": "OakERP.Local",
    "Audience": "OakERP.LocalUsers",
    "ExpireMinutes": 60
  },
  "Cors": {
    "AllowedOrigins": [
      "https://localhost:7094",
      "http://localhost:5067"
    ]
  }
}
```

### Web

- HTTPS: `https://localhost:7094`
- HTTP fallback: `http://localhost:5067`

Typical local Web override file:

```json
{
  "Api": {
    "BaseUrl": "https://localhost:7057"
  }
}
```

Use HTTPS by default. Keep HTTP only as a fallback while repairing localhost certificate trust.

## Running The Solution

### API

```powershell
dotnet run --project OakERP.API/OakERP.API.csproj
```

### Web

```powershell
dotnet run --project OakERP.Web/OakERP.Web.csproj
```

### Desktop / Mobile

Desktop and Mobile are MAUI hosts over the shared shell and shared MAUI host adapters. Run them from Visual Studio or the appropriate target-specific MAUI workflow once the required workloads are installed.

## Database And Migrations

- Docker Postgres host port: `5433`
- default local database: `oakerp`
- integration-test databases use `oakerp` and `oakerp_test`

Use:

- [db-setup.md](db-setup.md) for Docker and pgAdmin
- [ef-core.md](ef-core.md) for migration commands and connection strategies

## Testing

Default validation from the repo root:

```powershell
dotnet build OakERP.sln
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj
```

See [oakERP-testing-guide.md](oakERP-testing-guide.md) for the current unit, integration, runtime, posting, and Swagger testing split.

## Architecture And API References

- [architecture-overview.md](architecture-overview.md)
- [api-conventions.md](api-conventions.md)
- [../docs/architecture/dependency-rules.md](../docs/architecture/dependency-rules.md)
- [../docs/architecture/project-map.md](../docs/architecture/project-map.md)
- [../docs/ai/codex-workflow.md](../docs/ai/codex-workflow.md)

## Known Gaps

- Mobile is still narrower than Web/Desktop and should not be treated as feature-complete.
- The docs focus on the current backend and host architecture; broader product/user documentation is still to come.
