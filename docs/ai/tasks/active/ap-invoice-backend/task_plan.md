# Task Plan

## Task Name
ap-invoice-backend

## Goal
Implement the smallest safe backend-only AP invoice MVP:
- add `POST /api/ap-invoices`
- create draft AP invoices with lines
- validate vendor, document uniqueness, currency, GL accounts, totals, and v1 line restrictions
- persist invoice and lines transactionally
- add unit and integration tests in the same slice

## Background
OakERP is being built backend-first. AR invoice posting, AR receipt capture + allocation, and AR receipt posting already exist. AP invoice entities and persistence already exist, but there is no AP application service, API endpoint, DI registration, or tests for draft AP invoice capture.

## Scope
- `OakERP.Application/AccountsPayable`
- `OakERP.Infrastructure/Accounts_Payable`
- `OakERP.Domain/Repository_Interfaces/Accounts_Payable`
- `OakERP.Infrastructure/Repositories/Accounts_Payable`
- `OakERP.Infrastructure/Extensions`
- `OakERP.API/Controllers`
- `OakERP.API/Program.cs`
- `OakERP.Tests.Unit/AccountsPayable`
- `OakERP.Tests.Integration/AccountsPayable`
- `OakERP.Tests.Integration/ApiRoutes.cs`
- `docs/ai/tasks/active/ap-invoice-backend`

## Out of Scope
- AP invoice posting
- AP payment capture, allocation, or posting
- read/list/query endpoints
- edit/update/delete endpoints
- UI or screen-flow work
- schema changes unless a hard blocker appears
- inventory receipt, landed-cost, item-based AP behavior
- tax calculation or tax-rate support beyond explicit rejection in v1

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
- [x] Migration `Up()` / `Down()` symmetry reviewed if schema work is included
- [x] New domain-significant constants / enums documented if introduced
- [x] Transactional failure / rollback behavior validated if persistence behavior changed
- [x] Deferred smells / risks recorded if intentionally left unresolved

## Planned Steps
1. Audit current AP entities, repos, DB config, and existing AR service/controller/test patterns.
2. Add Application contracts, Infrastructure validator/service/snapshot seam, narrow AP repository helpers, DI wiring, and the AP invoice controller.
3. Add unit and integration tests, then run the required validation commands and update task docs with outcomes and deferrals.

## Validation Commands
```powershell
dotnet build OakERP.API/OakERP.API.csproj
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter FullyQualifiedName~AccountsPayable
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ApInvoice
pwsh ./tools/validate-pr.ps1
```

## Test Notes
This task requires both:
- unit tests for AP invoice validation and orchestration
- integration tests for API wiring, persistence, transaction behavior, and DTO failure responses

## Risks

- Currency validation now depends on `ICurrencyRepository` registration, which was previously present in code but not wired into DI.
- Item and tax-rate lines are explicitly rejected in v1 to avoid silently implying inventory or tax-engine support that the repo does not yet provide.

## Architecture Checks

- Are runtime models and persisted entity models still cleanly separated?
- Were business-significant literals centralized instead of repeated?
- Do the tests exercise real fallback paths instead of pre-resolved values?
- Did any new abstraction clearly solve a real problem?
- Did thin orchestrators stay thin and pure engines stay pure where intended?

## Notes

- Serena was not available in this session; discovery and implementation used direct file inspection.
- No schema change was required for this slice.
