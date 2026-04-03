# Findings

## Task
api-runtime-support

## Current State
- Slice 1 is complete and `OakERP.API` already wires centralized ProblemDetails, exception handling,
  correlation IDs, and request metadata logging.
- Slice 2 is complete and the API host already exposes `/health/live`, `/health/ready`, database
  connectivity readiness, and controller-focused request timeouts.
- Before Slice 3, the API host had no rate-limiter registration, no rate-limiter middleware, no
  auth endpoint rate-limit metadata, and no 429 response coverage.
- The only anonymous business endpoints in the current API surface are
  `POST /api/auth/register` and `POST /api/auth/login`, so they are the correct first protection
  target.
- Existing auth failures return DTO bodies via `BaseApiController.ApiResult(...)`; Slice 3 must
  preserve that behavior for non-throttled requests.

## Relevant Projects
- `OakERP.API`
- `OakERP.Tests.Integration`

## Dependency Observations
- Slice 3 remains API-host focused.
- No change is needed in `OakERP.Auth`, `OakERP.Infrastructure`, or application/domain layers.
- One API-local settings type is sufficient; no new shared abstraction is justified.

## Structural Problems Addressed
- Anonymous auth endpoints had no host-level throttling despite being the most obvious abuse surface.
- The runtime support suite had no end-to-end 429 coverage.
- Endpoint-specific rate limiting needed explicit routing in the current middleware order to make
  endpoint metadata effective.

## Configuration / Environment Notes
- The auth limiter should stay config-backed and host-local.
- Queueing remains fixed at `0` in this slice by design.
- `RemoteIpAddress` fallback stays deterministic and simple; no forwarded-header or proxy work is
  included here.

## Testing Notes
- Runtime tests now cover:
  - non-throttled auth DTO behavior
  - throttled 429 ProblemDetails behavior with correlation echo
  - separate login/register buckets under the same auth policy
  - health endpoint behavior after auth throttle exhaustion
- Attempting to lower the permit limit through a derived `WebApplicationFactory` config override was
  not reliable enough for this host-bound configuration path, so the final tests exhaust the
  configured permit limit from the actual API host instead.

## Rollback / Transaction Notes
- Migration rollback reviewed:
- Transactional failure leaves no writes:

## Deferred Smells / Risks
- Mixed error-shape model remains by design: DTO bodies for expected business failures, ProblemDetails
  for host/runtime failures such as 429s.
- No global limiter, no per-user/tenant limiter, and no proxy-aware client identification are added
  in this slice.
- `Retry-After` is written when the built-in metadata is available, but tests do not depend on it as
  a stable contract in the current harness.
- Audit logging remains deferred to a later slice.

## Recommendation
Keep future runtime-support work separate from this slice. The next operational addition should be
audit logging, not a broader rate-limit matrix.
