# Task Plan

## Task Name
api-runtime-support

## Goal
Implement Slice 1 only: add minimal API-host runtime support for centralized exception handling,
ProblemDetails for unhandled and empty framework-generated errors, simple request correlation, and
request logging without changing successful response bodies or existing DTO-based business failure
bodies.

## Background
OakERP.API currently has thin controller/auth wiring but no centralized host runtime support for
operability. Unhandled exceptions, empty framework-generated 401 responses, and request diagnostics
are not standardized, which makes the API harder to debug and operate before more feature work is
added.

## Scope
- `OakERP.API`
- `OakERP.Tests.Integration`
- `docs/ai/tasks/active/api-runtime-support/`
- small runtime-support config additions in `OakERP.API/appsettings*.json` only if needed for this slice

## Out of Scope
- redesigning existing controller success responses
- replacing current DTO-based business failure responses with ProblemDetails
- health checks, request timeouts, rate limiting, and audit logging
- Serilog, OpenTelemetry, or external logging packages
- request/response body logging
- controller-level try/catch additions

## Constraints
- Preserve behavior unless explicitly asked otherwise.
- Do not introduce secrets or hardcoded environment values.
- Keep the change set as small as possible.
- Prefer structural improvement over cosmetic churn.
- Add abstractions only when they solve a real coupling or duplication problem.
- Keep thin orchestrators thin and pure engines/calculators pure when that is part of the intended design.
- Review domain-significant magic numbers and strings.

## Success Criteria
- [ ] Slice 1 runtime support is implemented without broadening scope
- [ ] Unhandled exceptions are centrally translated to ProblemDetails
- [ ] Empty framework-generated auth/status errors are returned as ProblemDetails
- [ ] Correlation ID is accepted inbound or generated, echoed in the response, and included in log scope / ProblemDetails extensions
- [ ] Request logging captures method, path, status code, duration, correlation ID, and safe user context without logging bodies or secrets
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
1. Configure built-in ProblemDetails and a centralized exception handler in `OakERP.API`.
2. Add minimal correlation and request logging middleware in the API host only.
3. Add status-code ProblemDetails handling for empty framework-generated errors such as 401.
4. Add targeted integration tests for exception handling, unauthenticated responses, and correlation header behavior.
5. Run requested validation and update task docs with results and deferred risks.

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

- `UseStatusCodePages` must not overwrite existing DTO-based business failure bodies.
- Correlation/request logging must not capture authorization headers, tokens, passwords, or seed credentials.
- Middleware order mistakes could prevent exception handling, auth, or ProblemDetails from behaving consistently.

## Architecture Checks

- Are runtime models and persisted entity models still cleanly separated?
- Were business-significant literals centralized instead of repeated?
- Do the tests exercise real fallback paths instead of pre-resolved values?
- Did any new abstraction clearly solve a real problem?
- Did thin orchestrators stay thin and pure engines stay pure where intended?

## Notes

- This slice is intentionally host-focused and should stay inside `OakERP.API` except for tests.
- If consistent ProblemDetails would require changing current business DTO error contracts, stop and report the conflict instead of widening the slice.
