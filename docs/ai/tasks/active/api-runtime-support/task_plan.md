# Task Plan

## Task Name
api-runtime-support

## Goal
Implement Slice 4 only: add minimal log-backed audit logging for auth outcomes in `AuthService`
without introducing a new audit subsystem, changing API contracts, or touching persistence.

## Background
Slices 1-3 are already complete and provide:
- centralized exception handling
- ProblemDetails for unhandled and empty framework-generated errors
- correlation IDs
- request metadata logging
- `/health/live`
- `/health/ready`
- database readiness checks
- controller-focused request timeouts
- auth-endpoint rate limiting

The remaining runtime-support gap for this slice is a small, operationally useful audit trail for
the current security-sensitive auth actions.

## Scope
- `OakERP.API`
- `OakERP.Auth`
- `OakERP.Tests.Unit`
- `OakERP.Tests.Integration`
- `docs/ai/tasks/active/api-runtime-support/`

## Out of Scope
- audit database or persistent audit store
- generic audit interfaces, sinks, or event buses
- API contract changes
- DB schema or persistence changes
- audit coverage for non-auth endpoints
- external observability platforms

## Constraints
- Preserve behavior unless explicitly asked otherwise.
- Keep the slice narrow and log-backed only.
- Audit only auth actions in this slice: registration and login outcomes.
- Move the request log scope so downstream service logs inherit correlation/trace scope, but do not redesign the middleware.
- Keep audit logging local to `AuthService`.
- Do not log passwords, confirm passwords, JWTs, license keys, seed credentials, or raw DTOs.
- Keep tests practical and valuable.

## Success Criteria
- [x] Registration and login outcomes are logged as structured audit events
- [x] Audit logs remain local to `AuthService`
- [x] Downstream auth service logs inherit request `CorrelationId` and `TraceId`
- [x] DTO/API response bodies remain unchanged
- [x] No new audit abstraction, sink, or event bus is introduced
- [x] No DB schema or persistence behavior changes
- [x] Targeted unit and runtime tests pass
- [x] Docs are updated with validation results and deferred risks

## Planned Steps
1. Move the request log scope to wrap downstream execution so service logs inherit runtime correlation scope.
2. Add structured audit logging for registration and login outcomes inside `AuthService`.
3. Add practical unit tests for structured auth audit logs.
4. Add one targeted runtime/integration test to prove auth audit logs inherit correlation scope.
5. Run the requested validation commands and record real regressions or environment blockers.

## Validation Commands
```powershell
dotnet build OakERP.API/OakERP.API.csproj
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter AuthServiceTests
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~OakERP.Tests.Integration.Runtime
dotnet build OakERP.sln
```

## Test Notes
- unit tests are required for `AuthService` audit logging behavior
- one targeted runtime/integration test is required for correlation-scope inheritance
- no extra audit-persistence or end-to-end export testing is required in this slice

## Risks
- Audit logs could accidentally leak sensitive auth data if the slice logs DTOs or tokens instead of selected fields.
- Request-scope changes must not redesign the middleware or alter API behavior.
- The solution build may remain subject to machine workload prerequisites outside the backend runtime slice.

## Architecture Checks
- Auth audit logging stays in `OakERP.Auth`.
- HTTP/runtime scope ownership stays in `OakERP.API`.
- No new cross-layer audit abstraction is introduced.
- Deferred operational concerns are recorded instead of being broadened into this slice.
