# Findings

## Task
serilog-seq-logging

## Current State
- `OakERP.API` already owns runtime-support behavior through:
  - `CorrelationIdMiddleware`
  - `RequestLoggingMiddleware`
  - `GlobalExceptionHandler`
  - ProblemDetails customization
  - health checks
  - request timeouts
  - auth rate limiting
- `RequestLoggingMiddleware` and `GlobalExceptionHandler` establish correlation and trace context with
  `ILogger.BeginScope(...)`.
- `AuthService` audit logs and `SeedCoordinator` logs already use structured `ILogger<T>` message templates.
- Before this slice, the API host used default Microsoft logging only. There were no Serilog or Seq packages, no
  Serilog config section, and no local Seq service in `docker-compose.yml`.
- `docs/ai/tasks/active/` was missing on this branch and had to be recreated before task docs could be recorded.

## Relevant Projects
- `OakERP.API`
- `OakERP.Tests.Integration`
- `OakERP.Tests.Unit`
- `OakERP.Docs`

## Dependency Observations
- Serilog belongs at the API composition root for this slice.
- `Auth` and `Infrastructure` should keep depending only on `ILogger<T>` and must not take a direct Serilog
  dependency.
- Seq config is an API-host operational concern, not a shared cross-solution concern.

## Structural Problems
- Local console output was still using the default provider formatting, which is harder to scan during runtime-support
  and auth work.
- There was no optional in-repo local log UI for API development.
- Provider-based runtime tests needed explicit protection so Serilog adoption would not silently break custom logger
  capture.

## Literal / Model-Family Notes
- Repeated business-significant literals: none introduced by this slice.
- Repeated domain-significant numbers: none introduced by this slice.
- Runtime-vs-persisted model-family conflicts: none added.
- Thin orchestrators getting too thick: no change.
- Pure engines/calculators with side effects or lookups: no change.

## Configuration / Environment Notes
- Seq remains disabled by default in checked-in config.
- `OakErpWebFactory` now forces `Serilog:Seq:Enabled=false` so machine-level env vars do not bleed into tests.
- Local Seq support is exposed through `docker compose --profile seq`.
- No Seq API key is committed.

## Testing Notes
- Existing auth runtime tests already depended on a custom `ILoggerProvider`.
- Under Serilog with `writeToProviders: true`, provider capture still works, but correlation context is surfaced as
  structured event properties instead of only nested scope dictionaries. The updated runtime assertions account for
  that representation while still protecting the correlation contract.
- Existing auth unit tests remained valid because `AuthService` stayed on `ILogger<T>`.

## Rollback / Transaction Notes
- Migration rollback reviewed:
- Transactional failure leaves no writes:

## Open Questions
- None for this slice.

## Deferred Smells / Risks
- `OakERP.Client/Services/Api/ApiClient.cs` still logs raw failed response bodies; that remains out of scope here.
- Seq support is local/dev only. No production retention, dashboarding, alerting, or rollout policy is added.
- The runtime-support middleware stack remains intentionally unchanged; any future request-logging redesign should be a
  separate slice.

## Recommendation
Keep future observability work split into separate slices:
- local API logging infrastructure
- client logging/privacy cleanup
- production-grade telemetry or observability rollout
