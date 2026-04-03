# Task Plan

## Task Name
serilog-seq-logging

## Goal
Add Serilog-based API host logging with readable local console output and optional local/dev Seq support,
while preserving the current runtime-support behavior, existing `ILogger<T>` call sites, and provider-based
test capture.

## Background
The runtime-support slices are now in place in `OakERP.API`, including correlation IDs, request logging,
ProblemDetails, health checks, timeouts, rate limiting, and auth audit logging. This slice improves the host
logging infrastructure only; it does not redesign those behaviors.

## Scope
- `OakERP.API`
- `OakERP.Tests.Integration`
- `OakERP.Docs`
- repo-root `docker-compose.yml`
- `docs/ai/tasks/active/serilog-seq-logging/`

## Out of Scope
- `Auth`, `Infrastructure`, `Application`, `Domain`, `Web`, `Desktop`, or `Mobile` Serilog adoption
- runtime-support middleware redesign
- replacing `RequestLoggingMiddleware` with `UseSerilogRequestLogging()`
- OpenTelemetry
- production observability rollout
- logging request/response bodies, passwords, tokens, or license keys

## Constraints
- Preserve runtime behavior.
- Keep Serilog adoption API-host-only.
- Keep Seq optional and disabled by default.
- Preserve current correlation behavior driven by `BeginScope`.
- Keep provider-based test capture working under Serilog.
- Do not commit Seq API keys or machine-specific config.

## Success Criteria
- [x] `OakERP.API` uses Serilog as the host logger.
- [x] Existing `ILogger<T>` call sites remain unchanged.
- [x] Console logging is more readable for local runs.
- [x] Seq can be enabled by config without being required.
- [x] Runtime-support behavior remains intact.
- [x] Targeted integration coverage protects provider-based log capture under Serilog.
- [x] Task docs are created on this branch and updated with findings/results.

## Planned Steps
1. Recreate the active task folder for this slice because `docs/ai/tasks/active/` was missing on this branch.
2. Add Serilog packages and API-host configuration, including optional Seq settings and console sink wiring.
3. Keep middleware and application/service logging call sites unchanged.
4. Add targeted runtime test coverage for provider forwarding under Serilog and keep tests isolated from local Seq env vars.
5. Add local/dev Seq compose/docs guidance and run the requested validation commands.

## Validation Commands
```powershell
dotnet restore OakERP.API/OakERP.API.csproj
dotnet build OakERP.API/OakERP.API.csproj --no-restore
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~OakERP.Tests.Integration.Runtime
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter AuthServiceTests
pwsh ./tools/validate-pr.ps1
```

## Test Notes
This task requires both:
- unit tests to keep the auth audit log surface green
- integration tests to protect runtime/provider capture behavior and runtime wiring

## Risks
- Provider-forwarded capture under Serilog materializes correlation context as structured properties rather than nested
  test scope dictionaries; tests must assert the runtime contract, not the provider’s internal representation.
- Local/dev Seq support must stay opt-in so test and CI environments do not depend on a local log UI.

## Architecture Checks
- Runtime models and persisted entity models remain unchanged.
- No new domain-significant literals or enums were introduced.
- The only new abstraction is one API-local settings type for Seq config.
- Middleware ownership remains in `OakERP.API`.
- No new cross-layer dependency direction issue was introduced.
