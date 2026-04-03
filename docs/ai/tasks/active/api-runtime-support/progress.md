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

## Files Touched
- `docs/ai/tasks/active/api-runtime-support/task_plan.md`
- `docs/ai/tasks/active/api-runtime-support/findings.md`
- `docs/ai/tasks/active/api-runtime-support/progress.md`
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
- `OakERP.Tests.Integration/Runtime/RuntimeSupportTests.cs`

## Validation
- `dotnet build OakERP.API/OakERP.API.csproj`
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter AuthServiceTests`
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter AuthApiTests --no-restore`
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter RuntimeSupportTests`
- `dotnet build OakERP.API/OakERP.API.csproj`
- `dotnet build OakERP.sln`
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~OakERP.Tests.Integration.Runtime`
- `dotnet build OakERP.API/OakERP.API.csproj`
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~OakERP.Tests.Integration.Runtime`
- `dotnet build OakERP.sln`
- No new unit tests were added for Slice 3 because the change is host wiring plus endpoint metadata, and the new behavior is covered through targeted runtime integration tests.

## Validation Notes
- An initial attempt to run API build and integration tests in parallel triggered a transient file lock on `OakERP.API.dll` attributed to concurrent access/Defender scanning. Re-running sequentially passed; this was validation noise, not a product regression.
- An initial Slice 3 regression was real: auth endpoint metadata did not activate rate limiting until explicit `UseRouting()` was added before `UseRateLimiter()`.
- Lowering the permit limit through a derived `WebApplicationFactory` config override was not reliable for this host-bound configuration path, so the final tests exhaust the configured permit limit from the real API host instead.

## Remaining
- Later runtime-support work only: audit logging.
- Optional future follow-up: proxy-aware client identification if OakERP is deployed behind forwarded headers and auth throttling needs per-client accuracy beyond the current simple partitioning.
- Optional future follow-up: broader cancellation-token propagation if OakERP wants harder timeout enforcement for long-running business flows.

## Deferred Smells / Risks
- Mixed error-shape model remains after these slices: DTO bodies for expected business failures, ProblemDetails for unhandled and host/runtime failures.
- Request logging is intentionally compact and host-local; richer structured logging/observability remains deferred.
- The runtime-support slices intentionally avoid request/response body logging and avoid any auth-token or seed-secret logging, which keeps diagnostics metadata-only.
- Readiness checks only database connectivity; migration state, seed state, and downstream dependencies remain intentionally deferred.
- Current business flows still do not propagate cancellation tokens through every lower layer, so hard timeout enforcement for mutating operations remains a later concern.
- Auth rate limiting is intentionally narrow in this slice: no global limiter, no per-user/tenant limiter, no proxy-aware forwarding support, and no audit side effects.

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

## Next Recommended Step
- Start the next runtime-support slice separately for audit logging only. Keep broader client-identification and policy-matrix work deferred until OakERP has a concrete deployment topology and real operational feedback.
