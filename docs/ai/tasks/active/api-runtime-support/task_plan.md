# Task Plan

## Task Name
api-runtime-support

## Goal
Implement Slice 3 only: add minimal built-in rate limiting for the anonymous auth endpoints
without widening scope beyond `POST /api/auth/login` and `POST /api/auth/register`.

## Background
Slices 1 and 2 are complete and already provide:
- centralized exception handling
- ProblemDetails for unhandled and empty framework-generated errors
- correlation IDs
- request metadata logging
- `/health/live`
- `/health/ready`
- database connectivity readiness checks
- controller-focused request timeouts

The remaining runtime gap for this slice is simple operational protection for the public auth
surface.

## Scope
- `OakERP.API`
- `OakERP.Tests.Integration`
- `docs/ai/tasks/active/api-runtime-support/`
- small runtime-support config additions in `OakERP.API/appsettings.json`

## Out of Scope
- audit logging
- controller success or DTO error-contract redesign
- global API rate limiting
- multiple rate-limit tiers or policy matrices
- forwarded-header or proxy-aware client identification
- external observability, WAF, or CDN controls
- queue-based throttling

## Constraints
- Preserve behavior unless explicitly asked otherwise.
- Keep the slice host-focused and minimal.
- Use built-in ASP.NET Core facilities where possible.
- Keep the first policy auth-endpoint-only.
- Preserve current auth DTO failure bodies for non-throttled requests exactly.
- Keep queueing disabled.
- Write 429 failures through the existing ProblemDetails path so `correlationId` and `traceId` stay present.
- Do not broaden into audit logging in this slice.

## Success Criteria
- [x] One built-in fixed-window auth rate-limit policy is configured
- [x] Rate limiting applies only to `POST /api/auth/login` and `POST /api/auth/register`
- [x] Queueing remains disabled
- [x] Non-throttled auth requests keep their current DTO success/failure bodies
- [x] Throttled requests return `429` ProblemDetails with `correlationId` and `traceId`
- [x] Health endpoints remain unaffected
- [x] Relevant build passes
- [x] Targeted integration tests pass
- [x] Docs updated with results and deferred risks

## Planned Steps
1. Add one API-local settings type for auth rate-limiting configuration.
2. Register one built-in fixed-window policy plus a single rejection writer through the existing ProblemDetails service.
3. Apply auth-only rate-limit metadata at action level.
4. Add targeted integration tests for pre-limit DTO behavior, throttled 429 behavior, auth-path separation, and health-endpoint exclusion.
5. Run the requested validation commands and record any regressions vs harness noise.

## Validation Commands
```powershell
dotnet build OakERP.API/OakERP.API.csproj
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~OakERP.Tests.Integration.Runtime
dotnet build OakERP.sln
```

## Test Notes
- integration tests are required for runtime behavior in this slice
- unit tests are not required because no new pure helper seam is introduced

## Risks
- Endpoint-specific rate limiting depends on correct middleware ordering and available route metadata.
- Client partitioning remains intentionally simple in this slice and does not handle forwarded headers or proxy topologies.
- Aggressive future config changes could make auth throttling operationally noisy if not tuned with real traffic data.
- `Retry-After` metadata should be treated as best-effort in this slice, not a hard test contract.

## Architecture Checks
- Runtime support stays in `OakERP.API` as HTTP host policy.
- No new cross-layer abstraction was introduced.
- Existing controller/application contracts remain unchanged.
- Deferred operational concerns are recorded instead of silently widened into this slice.
