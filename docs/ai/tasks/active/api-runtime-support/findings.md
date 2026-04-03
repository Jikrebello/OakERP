# Findings

## Task
api-runtime-support

## Current State
- Slice 1 is complete and `OakERP.API` now wires centralized ProblemDetails, exception handling,
  correlation ID support, and request metadata logging.
- `OakERP.API` still does not register `AddHealthChecks`, `MapHealthChecks`, `AddRequestTimeouts`,
  `UseRequestTimeouts`, or any timeout policies.
- Controllers remain the only mapped business API surface; there are no current host-local endpoints.
- Business/application failures from auth flows still return DTO envelopes via `BaseApiController.ApiResult(...)`.
- `ApplicationDbContext` is already registered and available in the API host, so a database connectivity readiness check is feasible without changing dependency direction.
- During Slice 2 implementation, WebApplicationFactory/TestServer did not trigger an end-to-end timeout response even with a cooperative long-running test probe. The timeout response path is still testable directly by invoking the configured response writer, but a full in-harness 503 timeout response appears to be a harness limitation rather than a product regression.

## Relevant Projects
- `OakERP.API`
- `OakERP.Tests.Integration`
- `OakERP.Infrastructure` only as a dependency surface through the existing `ApplicationDbContext` registration; no planned logic changes there

## Dependency Observations
- Slice 2 can remain API-host focused.
- No new cross-layer abstraction is required; one API-local health check class is sufficient.

## Structural Problems
- No liveness or readiness endpoints exist for basic operational probing.
- No database readiness probe exists even though the API depends on database connectivity for normal work.
- No timeout guardrail exists for controller requests.

## Literal / Model-Family Notes
- Repeated business-significant literals:
- Repeated domain-significant numbers:
- Runtime-vs-persisted model-family conflicts:
- Thin orchestrators getting too thick:
- Pure engines/calculators with side effects or lookups:

## Configuration / Environment Notes
- Existing appsettings still contain localhost-oriented dev/test assumptions and seed credentials; Slice 2 must not add new hardcoded runtime secrets or machine-specific values.
- If a timeout default is introduced, it should be config-backed and host-local.

## Testing Notes
- Integration harness already exists via `WebApplicationFactory<Program>` and `Testing` environment.
- Existing runtime tests already cover Slice 1 exception/correlation behavior.
- No tests currently assert health endpoint behavior or timeout behavior.
- Health endpoint behavior is suitable for request/response integration tests.
- Timeout configuration and ProblemDetails response writing are covered in this slice, but a full end-to-end timeout response could not be reproduced inside the existing WebApplicationFactory/TestServer harness.

## Rollback / Transaction Notes
- Migration rollback reviewed:
- Transactional failure leaves no writes:

## Open Questions
- Resolved for Slice 2: readiness should check database connectivity only, and liveness must remain database-independent.

## Deferred Smells / Risks
- Existing DTO-based business failure contract remains mixed with ProblemDetails-based host/runtime errors by design in this slice.
- `BaseApiController` still has a nullable warning around `StatusCode`; if not touched by this slice it should remain documented, not silently widened into a response-contract cleanup.
- Request logging is intentionally minimal and host-local. Richer observability, log sinks, and cross-service tracing remain deferred to later slices.
- Health checks should stay minimal in this slice; richer dependency graphs, migration-state checks, and platform integrations remain deferred.
- Timeout policy complexity should stay deferred: no multiple tiers, attributes, or endpoint-specific exceptions in this slice.
- Current controllers/services do not uniformly flow cancellation tokens through the application stack, so timeout effectiveness for long-running business work remains partly dependent on later cooperative-cancellation cleanup.

## Recommendation
Implement a small API-host slice using built-in health checks plus one controller-only built-in request timeout policy, then cover it with targeted integration tests before moving on to rate limiting or audit logging.

