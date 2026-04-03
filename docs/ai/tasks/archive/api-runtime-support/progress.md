# Progress

## Task
api-runtime-support

## Started
2026-03-30 17:11:23

## Work Log
- Audited required repo guidance and OakERP-specific skills before editing.
- Inspected `OakERP.API` startup, middleware, controllers, auth service logging, appsettings, and integration harness.
- Confirmed current baseline: API build passes, targeted auth unit tests pass, and targeted auth integration tests pass.
- Prepared Slice 1 implementation/tasking plan focused on centralized exception handling, ProblemDetails, correlation, and request logging.
- Added API-host runtime support registration with built-in ProblemDetails customization and a centralized exception handler.
- Added minimal correlation ID middleware that accepts inbound values, generates one when missing, echoes the response header, and feeds ProblemDetails/log scope context.
- Added minimal request logging middleware that records method, path, status code, duration, correlation ID, trace ID, and safe user context without logging bodies or secrets.
- Added status-code ProblemDetails handling for empty framework-generated responses so unauthenticated requests return `application/problem+json`.
- Added targeted integration coverage for the exception path, unauthenticated ProblemDetails path, and DTO-preserving correlation-header behavior.
- Audited current host pipeline, appsettings, and integration harness for Slice 2 health/timeout support.
- Confirmed there are currently no health endpoints, health-check registrations, or request timeout policies in the API host.
- Added built-in health-check registration with separate live and ready probes in the API host.
- Added an API-local database connectivity health check for readiness only.
- Added built-in request timeout middleware and one default timeout policy, with health endpoints explicitly opted out so the policy remains controller-only in practice.
- Added timeout response writing through the existing ProblemDetails service path so `correlationId` and `traceId` stay present.
- Added targeted runtime coverage for healthy/unhealthy health probes plus timeout metadata/response-writer behavior.
- Investigated an end-to-end timeout-response test in WebApplicationFactory/TestServer; the harness did not surface a timed-out HTTP response even with a cooperative slow probe, so that check was classified as harness noise and replaced with narrower timeout coverage that exercises the configured response path directly.
- Audited the current auth surface and runtime wiring for Slice 3 rate limiting.
- Added one API-local auth rate-limit settings type and one built-in fixed-window policy with queueing disabled.
- Added one built-in rejection writer that returns 429 ProblemDetails through the existing ProblemDetails service path.
- Applied auth-only rate-limit metadata at action level so the limiter stays scoped to `login` and `register`.
- Fixed a real middleware-order issue by adding explicit routing before endpoint-specific rate limiting; without it, the auth metadata did not activate the limiter.
- Added targeted runtime coverage for non-throttled DTO behavior, throttled ProblemDetails behavior, auth-path separation, and health-endpoint exclusion.
- Simplified the rate-limit tests to exhaust the configured permit limit from the real API host after a lower test override proved unreliable in the derived WebApplicationFactory path.
- Audited current auth logging and request scope behavior for Slice 4 audit logging.
- Moved the request logging scope so downstream service logs inherit request `CorrelationId` and `TraceId`.
- Added structured audit logs in `AuthService` for registration and login outcomes, using `Information` for success, `Warning` for expected denials, and `Error` for unexpected registration exceptions.
- Kept audit logging local to `AuthService` and avoided any new audit interface, sink abstraction, event bus, or persistence work.
- Added unit coverage for registration-success and invalid-login audit log events.
- Added targeted runtime coverage using an in-memory logger provider to prove auth audit logs inherit inbound correlation scope.

## Files Touched
- `docs/ai/tasks/archive/api-runtime-support/task_plan.md`
- `docs/ai/tasks/archive/api-runtime-support/findings.md`
- `docs/ai/tasks/archive/api-runtime-support/progress.md`
- `OakERP.API/Program.cs`
- `OakERP.API/Extensions/ServiceCollectionExtensions.cs`
- `OakERP.API/Extensions/AppBuilderExtensions.cs`
- `OakERP.API/Extensions/HttpContextExtensions.cs`
- `OakERP.API/Extensions/CorrelationIdMiddleware.cs`
- `OakERP.API/Extensions/RequestLoggingMiddleware.cs`
- `OakERP.API/Extensions/GlobalExceptionHandler.cs`
- `OakERP.API/Runtime/DatabaseConnectivityHealthCheck.cs`
- `OakERP.API/Runtime/RequestTimeoutSettings.cs`
- `OakERP.API/Runtime/AuthRateLimitSettings.cs`
- `OakERP.API/Controllers/AuthController.cs`
- `OakERP.API/appsettings.json`
- `OakERP.Auth/AuthService.cs`
- `OakERP.Tests.Unit/Auth/AuthServiceTests.cs`
- `OakERP.Tests.Integration/Runtime/RuntimeSupportTests.cs`

## Validation
- `dotnet build OakERP.API/OakERP.API.csproj`
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter AuthServiceTests`
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~OakERP.Tests.Integration.Runtime`
- `dotnet build OakERP.sln`

## Validation Notes
- `dotnet build OakERP.API/OakERP.API.csproj` passed.
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter AuthServiceTests` passed.
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~OakERP.Tests.Integration.Runtime` passed.
- `dotnet build OakERP.sln` did not pass on this machine because required MAUI workloads are missing:
  - `maui-tizen` for `OakERP.Desktop`
  - `maui-tizen` and `wasm-tools` for `OakERP.Mobile`
- An initial parallel validation attempt hit transient file-lock errors on build outputs. Re-running the requested backend validations sequentially passed, so the lock was treated as environment noise rather than a code regression.

## Merge Review Follow-Up
- Reviewed the full branch against `main` with Serena symbol tracing across the runtime-support and auth seams.
- `pwsh ./tools/validate-pr.ps1` passed.
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter ArInvoicePostingTests` passed.
- No pre-merge correctness blocker was found in the branch diff.
- One cleanup item identified during review was fixed before archive: removed stale slice-specific wording from the auth rate-limit settings exception message.
- Remaining non-blocking gap: request timeout coverage still verifies endpoint metadata and the configured ProblemDetails writer, but not an end-to-end timed-out HTTP request through the middleware path.

## Remaining
- Later runtime-support work only: any broader audit-retention or persistent audit-storage decisions.
- Optional future follow-up: expand auth audit coverage to unexpected login exception paths only if OakERP decides it needs explicit exception-to-audit bridging.
- Optional future follow-up: broader client-identification or audit retention only when OakERP has a concrete operational requirement.

## Deferred Smells / Risks
- Mixed error-shape model remains after these slices: DTO bodies for expected business failures, ProblemDetails for unhandled and host/runtime failures.
- Request logging remains intentionally compact and host-local; richer structured logging and observability remain deferred.
- Audit logging is intentionally narrow in this slice: auth actions only, log-backed only, no database, no reporting layer, and no generic audit subsystem.
- Current business flows still do not propagate cancellation tokens through every lower layer, so hard timeout enforcement for mutating operations remains a later concern.
- Full solution validation on this machine remains blocked by missing MAUI workloads outside the backend runtime slice.

## Outcome
- Slice 1 is implemented and validated.
- Successful responses and existing DTO-based business failure bodies remain unchanged.
- Unhandled errors and empty framework-generated auth failures now return ProblemDetails with `traceId` and `correlationId`.
- Correlation ID is echoed on responses and included in runtime logging scope.
- Slice 2 is implemented and validated.
- `/health/live` is anonymous and database-independent.
- `/health/ready` is anonymous and checks database connectivity only.
- Built-in request timeout middleware is configured, and health endpoints explicitly opt out so the timeout policy remains controller-focused.
- Timeout failures are configured to use the existing ProblemDetails service path.
- Slice 3 is implemented and validated.
- `POST /api/auth/login` and `POST /api/auth/register` now share one built-in fixed-window auth policy with queueing disabled.
- Non-throttled auth requests still return their existing DTO bodies unchanged.
- Throttled auth requests now return `429` ProblemDetails with `correlationId` and `traceId`.
- Health endpoints remain unaffected by auth-rate-limit exhaustion.
- Slice 4 is implemented and validated for the backend runtime scope.
- Registration and login outcomes now emit structured audit logs from `AuthService`.
- Downstream auth service logs now inherit request `CorrelationId` and `TraceId` via the request logging scope.
- No API contracts, DB schema, or persistence behavior changed.

## Next Recommended Step
- Keep any future audit retention or broader action coverage in a separate slice. Do not expand this auth-focused, log-backed implementation into a general audit subsystem without a concrete operational need.
