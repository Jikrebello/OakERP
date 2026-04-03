# Progress

## Task
serilog-seq-logging

## Started
2026-04-03 14:16:33

## Work Log
- Re-read the required OakERP repo guidance and confirmed Serena MCP tools were not available in this session.
- Audited the updated runtime-support implementation and confirmed the key compatibility point is `BeginScope`-based
  correlation in API middleware.
- Recreated `docs/ai/tasks/active/serilog-seq-logging/` because the active task directory tree was missing on this
  branch.
- Added Serilog packages to `OakERP.API` only.
- Configured API-host Serilog wiring in `Program.cs` with:
  - config-backed minimum levels
  - readable console sink output
  - `Enrich.FromLogContext()`
  - static `Application=OakERP.API`
  - optional Seq sink
  - `writeToProviders: true` so provider-based test capture still works
- Added one API-local `SeqLoggingSettings` type to bind and validate `Serilog:Seq`.
- Added checked-in Serilog config defaults with Seq disabled by default.
- Forced `Serilog:Seq:Enabled=false` in the test factory to avoid machine-specific environment leakage.
- Added a targeted runtime test that proves request and auth logs still reach an injected provider under Serilog.
- Updated the existing auth runtime correlation test to assert the preserved correlation contract across Serilog’s
  provider-forwarded representation.
- Added optional local Seq service wiring to `docker-compose.yml` behind a `seq` profile.
- Updated `OakERP.Docs/db-setup.md` with concise local Seq startup guidance and env-var enablement.
- Fixed CSharpier formatting issues and reran the full validation path cleanly.

## Files Touched
- `docs/ai/tasks/active/serilog-seq-logging/task_plan.md`
- `docs/ai/tasks/active/serilog-seq-logging/findings.md`
- `docs/ai/tasks/active/serilog-seq-logging/progress.md`
- `OakERP.API/OakERP.API.csproj`
- `OakERP.API/Program.cs`
- `OakERP.API/Runtime/SeqLoggingSettings.cs`
- `OakERP.API/appsettings.json`
- `OakERP.API/appsettings.Development.json`
- `OakERP.API/appsettings.Testing.json`
- `OakERP.Tests.Integration/TestSetup/OakErpWebFactory.cs`
- `OakERP.Tests.Integration/Runtime/RuntimeSupportTests.cs`
- `OakERP.Docs/db-setup.md`
- `docker-compose.yml`

## Validation
- `dotnet restore OakERP.API/OakERP.API.csproj`
- `dotnet build OakERP.API/OakERP.API.csproj --no-restore`
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~OakERP.Tests.Integration.Runtime`
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter AuthServiceTests`
- `pwsh ./tools/validate-pr.ps1`

## Validation Notes
- All requested validation commands passed.
- An earlier `pwsh ./tools/validate-pr.ps1` run exposed CSharpier formatting issues in the new Serilog/test edits.
  Those files were formatted and the full validation path was rerun successfully.
- The targeted `dotnet build` command still reports a pre-existing nullable warning in `OakERP.API/Controllers/BaseController.cs`.
  This slice did not touch that file or broaden scope to resolve it.

## Remaining
- No required work remains within this slice.

## Deferred Smells / Risks
- Client-side raw response-body logging remains out of scope.
- Seq remains local/dev only; no production rollout or API key management is added.
- The runtime-support middleware design remains intentionally unchanged.

## Outcome
- `OakERP.API` now uses Serilog without changing existing application/service log call sites.
- Local console output is more readable and still carries structured properties.
- Seq can be enabled for local/dev use through config and a compose profile, but stays disabled by default.
- Existing runtime-support behavior and auth audit behavior remain intact.
- Provider-based runtime test capture remains protected under Serilog.

## Next Recommended Step
- Keep future work separate if OakERP later wants:
  - client logging/privacy cleanup
  - production observability rollout
  - OpenTelemetry/tracing/metrics adoption
