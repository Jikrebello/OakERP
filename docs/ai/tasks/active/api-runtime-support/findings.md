# Findings

## Task
api-runtime-support

## Current State
- `OakERP.API` currently wires controllers, JWT auth, CORS, Swagger, and startup seeding.
- `OakERP.API.Extensions.AppBuilderExtensions.UseOakMiddleware()` currently only adds authentication and authorization.
- `OakERP.API` does not currently register `AddProblemDetails`, `AddExceptionHandler`, health checks, rate limiting, or timeout policies.
- Business/application failures from auth flows currently return DTO envelopes via `BaseApiController.ApiResult(...)`.
- Framework-generated auth failures such as unauthorized access currently rely on default ASP.NET Core behavior.
- `AuthService` and `SeedCoordinator` already use `ILogger<T>` with structured message templates.

## Relevant Projects
- `OakERP.API`
- `OakERP.Tests.Integration`
- `OakERP.Auth` only as a dependency surface used by tests; no planned logic changes there

## Dependency Observations
- Slice 1 can remain API-host focused.
- No new cross-layer abstraction is required unless a tiny test seam is needed for exception-path integration testing.

## Structural Problems
- No centralized exception-to-response policy exists for unhandled API errors.
- No correlation ID support exists in the request pipeline.
- No request logging exists at the API host boundary.
- Non-business errors are not consistently shaped for clients.

## Literal / Model-Family Notes
- Repeated business-significant literals:
- Repeated domain-significant numbers:
- Runtime-vs-persisted model-family conflicts:
- Thin orchestrators getting too thick:
- Pure engines/calculators with side effects or lookups:

## Configuration / Environment Notes
- Existing appsettings still contain localhost-oriented dev/test assumptions and seed credentials; Slice 1 must not add new hardcoded runtime secrets or machine-specific values.
- Correlation ID support can stay non-configurable in this slice to avoid unnecessary options churn.

## Testing Notes
- Integration harness already exists via `WebApplicationFactory<Program>` and `Testing` environment.
- Baseline targeted auth integration tests pass before Slice 1 changes.
- Existing tests do not currently assert ProblemDetails, correlation headers, or centralized exception behavior.

## Rollback / Transaction Notes
- Migration rollback reviewed:
- Transactional failure leaves no writes:

## Open Questions
- Resolved for Slice 1: empty framework-generated status responses can be shaped with ProblemDetails as long as existing DTO-based business failure bodies are left untouched.

## Deferred Smells / Risks
- Existing DTO-based business failure contract remains mixed with ProblemDetails-based host/runtime errors by design in this slice.
- `BaseApiController` still has a nullable warning around `StatusCode`; if not touched by this slice it should remain documented, not silently widened into a response-contract cleanup.
- Request logging is intentionally minimal and host-local. Richer observability, log sinks, and cross-service tracing remain deferred to later slices.

## Recommendation
Implement a small API-host slice using built-in ASP.NET Core ProblemDetails and exception handling plus minimal custom middleware for correlation and request logging, then cover it with targeted integration tests before moving on to later runtime-support slices.

