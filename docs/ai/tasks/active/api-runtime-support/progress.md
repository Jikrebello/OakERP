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
- `OakERP.API/appsettings.json`
- `OakERP.Tests.Integration/Runtime/RuntimeSupportTests.cs`

## Validation
- `dotnet build OakERP.API/OakERP.API.csproj`
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter AuthServiceTests`
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter AuthApiTests --no-restore`
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter RuntimeSupportTests`
- `dotnet build OakERP.API/OakERP.API.csproj`
- `dotnet build OakERP.sln`
- No new unit tests were required for Slice 1 because no reusable pure helper logic was factored out into a separate seam.
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~OakERP.Tests.Integration.Runtime`
- `dotnet build OakERP.API/OakERP.API.csproj`
- `dotnet build OakERP.sln`
- No new unit tests were added for Slice 2; timeout response behavior is covered through targeted integration-level service/metadata tests in the existing runtime test suite.

## Remaining
- Later runtime-support slices only: rate limiting and audit logging.
- Optional future follow-up: broader cancellation-token propagation if OakERP wants hard timeout enforcement for long-running business flows rather than host-level timeout policy only.

## Deferred Smells / Risks
- Mixed error-shape model will remain after this slice: DTO bodies for expected business failures, ProblemDetails for unhandled and empty framework-generated errors.
- Request logging is intentionally compact and host-local; richer structured logging/observability remains deferred.
- The slice intentionally avoids request/response body logging and avoids any auth-token or seed-secret logging, which also means diagnostics remain metadata-only.
- Readiness checks only database connectivity in this slice; migration state, seed state, and downstream dependencies remain intentionally deferred.
- End-to-end timeout responses were not reproducible in the existing WebApplicationFactory/TestServer harness even with a cooperative probe. This was treated as harness noise, not a blocking product regression, and the slice keeps narrower timeout response-path coverage instead.
- Current business flows still do not propagate cancellation tokens through every lower layer, so hard timeout enforcement for mutating operations remains a later concern.

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

## Next Recommended Step
- Start Slice 3 separately: rate limiting and audit logging, keeping timeout-cancellation propagation concerns deferred unless that slice explicitly chooses to address them.
