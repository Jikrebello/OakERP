# Findings

## Task
api-runtime-support

## Current State
- Slice 1 is complete and `OakERP.API` already provides centralized ProblemDetails, exception handling,
  correlation IDs, and request metadata logging.
- Slice 2 is complete and the API host already exposes `/health/live`, `/health/ready`, database
  connectivity readiness, and controller-focused request timeouts.
- Slice 3 is complete and the API host already applies fixed-window rate limiting to `login` and
  `register`.
- Before Slice 4, `AuthService.RegisterAsync(...)` logged one success message and one unexpected
  exception message, while `LoginAsync(...)` logged no explicit outcomes.
- Before Slice 4, `RequestLoggingMiddleware` created its log scope after `await next(context)`, so
  downstream service logs did not inherit request `CorrelationId` or `TraceId`.

## Relevant Projects
- `OakERP.API`
- `OakERP.Auth`
- `OakERP.Tests.Unit`
- `OakERP.Tests.Integration`

## Dependency Observations
- Audit logging belongs in `OakERP.Auth` for this slice because the auth service already owns the
  login/registration decision points.
- Correlation and request scope remain API-host concerns and should not be re-plumbed into
  `AuthService` through `HttpContext` dependencies.
- No new shared audit abstraction is justified for the current API surface.

## Structural Problems Addressed
- Auth outcome logging was inconsistent and incomplete.
- Security-relevant failures such as invalid credentials, license rejection, and registration
  rejection had no explicit structured audit log.
- Request scope timing prevented service-level logs from inheriting correlation and trace context.

## Configuration / Environment Notes
- No new config is required for this slice.
- Audit logging remains log-backed only and uses the existing logging pipeline.
- No external observability or sink configuration is introduced.

## Testing Notes
- Unit tests are practical for `AuthService` because the existing factory already exposes
  `Mock<ILogger<AuthService>>`.
- A runtime/integration test is practical for correlation inheritance by using a local in-memory
  logger provider in the existing `WebApplicationFactory` harness.

## Rollback / Transaction Notes
- Migration rollback reviewed:
- Transactional failure leaves no writes:

## Deferred Smells / Risks
- Mixed error-shape model remains by design: DTO bodies for expected business failures, ProblemDetails
  for host/runtime failures.
- Audit logging remains intentionally narrow: auth actions only, log-backed only, no persistence or
  reporting layer.
- Unexpected login exceptions still rely on the existing global exception logging path; this slice
  does not add a broader exception-to-audit bridge.
- Request timeout coverage remains partial: the slice verifies endpoint metadata and the configured
  timeout ProblemDetails writer, but not a full end-to-end timed-out HTTP request in the current
  `WebApplicationFactory` harness.
- Solution-wide builds on this machine remain dependent on MAUI workloads outside the backend runtime
  slice.

## Recommendation
Keep future audit work separate from this slice. If OakERP later needs persistent audit retention or
broader action coverage, that should be a deliberate new slice rather than an expansion of this
minimal auth-focused implementation.

## Merge Review Outcome
- Reviewed the branch against `main` before merge.
- No must-fix correctness issue was found in the runtime-support branch.
- One should-fix cleanup issue found during review was the stale `Slice 3` wording in
  `AuthRateLimitSettings`; that wording was corrected before archive.
- The branch is clean enough to merge from a backend/runtime-support perspective.
