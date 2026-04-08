# Task Plan

## Task Name
refactor-waves-8-11

## Goal
Implement the next maintainability waves in order:
- decouple Application/Auth from HTTP semantics
- finish the current backend service decomposition pass
- unify client host composition across Web/Desktop/Mobile
- split oversized posting/runtime test files into smaller concern-based files

## Background
Recent cleanup waves improved architecture, but three problems still remained:
- Application/Auth failures still leaked HTTP semantics
- AP/AR/Auth orchestration was still too concentrated in service classes
- Web/Desktop/Mobile still had divergent client host bootstrap paths
- large posting/runtime test files were expensive to navigate and maintain

## Scope
- `OakERP.Common`
- `OakERP.Application`
- `OakERP.Auth`
- `OakERP.API`
- `OakERP.Shared`
- `OakERP.Web`
- `OakERP.Desktop`
- `OakERP.Mobile`
- `OakERP.Tests.Unit`
- `OakERP.Tests.Integration`
- `docs/ai/tasks/active/refactor-waves-8-11`

## Out of Scope
- database schema or migrations
- HTTP route shapes
- DTO wire contracts
- auth token format
- posting engine behavior

## Constraints
- Preserve behavior unless explicitly asked otherwise.
- Do not introduce secrets or hardcoded environment values.
- Keep the change set as small as possible.
- Prefer structural improvement over cosmetic churn.
- Add abstractions only when they solve a real coupling or duplication problem.
- Keep thin orchestrators thin and pure engines/calculators pure when that is part of the intended design.
- Review domain-significant magic numbers and strings.

## Success Criteria
- [x] The target issue is addressed
- [x] Relevant build passes
- [x] Relevant tests pass
- [x] Docs updated if needed
- [x] Remaining risks are documented
- [x] Required unit tests added or updated
- [x] Required integration tests added or updated
- [ ] Migration `Up()` / `Down()` symmetry reviewed if schema work is included
- [x] New domain-significant constants / enums documented if introduced
- [x] Transactional failure / rollback behavior validated if persistence behavior changed
- [x] Deferred smells / risks recorded if intentionally left unresolved

## Planned Steps
1. Replace `HttpStatusCode`-shaped result and exception semantics with application-local failure kinds plus API-side HTTP mapping.
2. Thin the auth/AP/AR invoice-payment-receipt services by extracting internal workflow collaborators and wire Auth/JWT through the shared clock seam.
3. Add a shared host bootstrap for client/auth/UI registration and align Mobile to the same path as Web/Desktop.
4. Split the oversized posting/runtime test files into smaller concern-based files and keep test coverage green.

## Validation Commands
```powershell
dotnet build OakERP.Auth/OakERP.Auth.csproj /nr:false /m:1
dotnet build OakERP.Application/OakERP.Application.csproj /nr:false /m:1
dotnet build OakERP.Shared/OakERP.Shared.csproj /nr:false /m:1
dotnet build OakERP.Web/OakERP.Web.csproj /nr:false /m:1
dotnet build OakERP.Desktop/OakERP.Desktop.csproj /nr:false /m:1
dotnet build OakERP.Mobile/OakERP.Mobile.csproj /nr:false /m:1
dotnet build OakERP.Tests.Unit/OakERP.Tests.Unit.csproj /nr:false /m:1
dotnet build OakERP.Tests.Integration/OakERP.Tests.Integration.csproj /nr:false /m:1
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj /nr:false /m:1
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj /nr:false /m:1
dotnet build OakERP.sln /nr:false /m:1
```

## Test Notes
State whether this task requires:
- both

## Risks

- runtime helper type sharing across split integration files had to stay centralized to avoid duplicate definitions
- service decomposition is intentionally collaborator-based, not a full domain-use-case redesign

## Architecture Checks

- Are runtime models and persisted entity models still cleanly separated?
- Were business-significant literals centralized instead of repeated?
- Do the tests exercise real fallback paths instead of pre-resolved values?
- Did any new abstraction clearly solve a real problem?
- Did thin orchestrators stay thin and pure engines stay pure where intended?

## Notes

- No schema work was performed.
- The new `FailureKind`-based error model preserves DTO wire shape by keeping HTTP mapping at the API edge.
