# Task Plan

## Task Name
api-runtime-support

## Goal
Implement Slice 2 only: add minimal API-host health checks and one controller-only request timeout
policy without redesigning existing API contracts or broadening into later runtime-support work.

## Background
Slice 1 is complete and already provides centralized exception handling, ProblemDetails for
unhandled and empty framework-generated errors, correlation IDs, and request metadata logging.
OakERP.API still lacks basic operational endpoints and timeout guardrails, so Slice 2 should add the
smallest practical live/readiness checks plus one conservative controller timeout policy.

## Scope
- `OakERP.API`
- `OakERP.Tests.Integration`
- `docs/ai/tasks/active/api-runtime-support/`
- small runtime-support config additions in `OakERP.API/appsettings*.json` only if needed for this slice

## Out of Scope
- redesigning existing controller success responses
- replacing current DTO-based business failure responses with ProblemDetails
- rate limiting and audit logging
- Serilog, OpenTelemetry, or external logging packages
- request/response body logging
- controller-level timeout attributes or multiple timeout tiers
- provider-specific health-check packages

## Constraints
- Preserve behavior unless explicitly asked otherwise.
- Do not introduce secrets or hardcoded environment values.
- Keep the change set as small as possible.
- Prefer structural improvement over cosmetic churn.
- Add abstractions only when they solve a real coupling or duplication problem.
- Keep thin orchestrators thin and pure engines/calculators pure when that is part of the intended design.
- Review domain-significant magic numbers and strings.

## Success Criteria
- [ ] Slice 2 runtime support is implemented without broadening scope
- [ ] `/health/live` is anonymous and fully DB-independent
- [ ] `/health/ready` is anonymous and checks database connectivity only
- [ ] Health endpoints remain host-local and do not change controller contracts
- [ ] A single controller-only timeout policy is configured using built-in ASP.NET Core facilities
- [ ] Timeout failures are written through the existing ProblemDetails path so `correlationId` and `traceId` remain present
- [ ] Health endpoints are not subject to the controller timeout policy
- [ ] Relevant build passes
- [ ] Relevant tests pass
- [ ] Docs updated if needed
- [ ] Remaining risks are documented
- [ ] Required unit tests added or updated if helper logic is factored out
- [ ] Required integration tests added or updated
- [ ] Migration `Up()` / `Down()` symmetry reviewed if schema work is included
- [ ] New domain-significant constants / enums documented if introduced
- [ ] Transactional failure / rollback behavior validated if persistence behavior changed
- [ ] Deferred smells / risks recorded if intentionally left unresolved

## Planned Steps
1. Add built-in health-check registration in `OakERP.API` with separate live and ready probes.
2. Implement one API-local database connectivity health check for readiness only.
3. Add one built-in controller timeout policy and map it to controllers only.
4. Write timeout failures through the existing ProblemDetails service path.
5. Add targeted integration tests for live/readiness health and timeout behavior.
6. Run requested validation and update task docs with results and deferred risks.

## Validation Commands
```powershell
dotnet build OakERP.API/OakERP.API.csproj
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter RuntimeSupportTests
dotnet build OakERP.sln
```

## Test Notes
State whether this task requires:
- integration tests for runtime behavior are required
- unit tests are only required if pure helper logic is extracted into testable helpers

## Risks

- Liveness must not accidentally depend on the database.
- Readiness must stay limited to connectivity only and not grow into migration or seeding state checks.
- Timeout policy wiring must not affect health endpoints or redesign existing controller contracts.
- Timeout failures must not bypass the existing ProblemDetails path.

## Architecture Checks

- Are runtime models and persisted entity models still cleanly separated?
- Were business-significant literals centralized instead of repeated?
- Do the tests exercise real fallback paths instead of pre-resolved values?
- Did any new abstraction clearly solve a real problem?
- Did thin orchestrators stay thin and pure engines stay pure where intended?

## Notes

- This slice is intentionally host-focused and should stay inside `OakERP.API` except for tests.
- If timeout handling would require broader controller/action contract changes, stop and report the conflict instead of widening the slice.
