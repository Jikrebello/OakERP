# OakERP Testing Guide

OakERP currently uses two test projects with a clear split:

- `OakERP.Tests.Unit`: fast isolated tests
- `OakERP.Tests.Integration`: API/runtime/persistence integration tests

## Current Test Stack

### Unit tests

- project: `OakERP.Tests.Unit`
- framework: `xUnit`
- common libraries: `Moq`, `Shouldly`

Typical coverage:

- application services and workflows
- auth services and seams
- settlement helpers
- posting engines and posting support
- architecture and options-binding tests
- API-local seams such as exception handling

### Integration tests

- project: `OakERP.Tests.Integration`
- framework: `NUnit`
- common libraries: `WebApplicationFactory`, `Respawn`, `Shouldly`

Typical coverage:

- auth API flows
- AP/AR API flows
- posting end-to-end behavior
- runtime middleware and health/rate-limit behavior
- Swagger/OpenAPI smoke coverage
- persistence and seeding through the real test harness

## Test Harness Shape

Important integration-test pieces:

- `TestSetup/OakErpWebFactory.cs`
- `TestSetup/WebApiIntegrationTestBase.cs`
- `TestSetup/TestConfiguration.cs`
- `TestSetup/TestConfigurationDefaults.cs`
- `TestSetup/TestDatabaseReset.cs`

The integration host runs the API in the `Testing` environment and injects deterministic defaults for:

- CORS
- JWT settings
- seed credentials
- test database connection strings

Current default test DB values:

- transactional DB: `oakerp`
- reset DB: `oakerp_test`
- host: `localhost`
- port: `5433`

## Running Tests

From the repo root:

### Unit

```powershell
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj
```

### Integration

```powershell
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj
```

### Typical backend validation

```powershell
dotnet build OakERP.sln
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj
```

For Codex/PR-ready backend validation, also see:

- `tools/validate-pr.ps1`

## What To Test Where

### Use unit tests for

- pure logic
- service/workflow behavior with mocked dependencies
- error mapping and orchestration seams
- options validation and architecture guards

### Use integration tests for

- HTTP endpoints and controller behavior
- persistence and transaction boundaries
- seeding/runtime wiring
- Identity/JWT behavior
- middleware/runtime support
- generated Swagger/OpenAPI behavior

## Current Test Areas

### Unit test areas

- `AccountsPayable`
- `AccountsReceivable`
- `Auth`
- `Posting`
- `Settlements`
- `Common/Orchestration`
- `Runtime`
- `Architecture`
- `Configuration`

### Integration test areas

- `Auth`
- `AccountsPayable`
- `AccountsReceivable`
- `Posting`
- `Runtime`
- `Swagger`

## Guidelines

- build passing is not enough when behavior changes
- use real persistence behavior in integration tests instead of mocked EF queries
- keep unit tests isolated and fast
- clean up or reset integration data through the existing harness, not ad hoc scripts
- add or update tests when you introduce a new workflow, seam, runtime behavior, or API contract

## Troubleshooting

### Integration tests fail on database setup

- confirm Docker is running
- confirm Postgres is reachable on `localhost:5433`
- confirm the test databases exist and the reset path can reach them

### File-lock noise during `dotnet` commands

If Windows/Defender causes transient file locks, rerun sequentially:

```powershell
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj /m:1 /nr:false
```

### HTTPS-related API integration issues

For local browser/API certificate setup, see:

- [local-https-dev-cert.md](local-https-dev-cert.md)
