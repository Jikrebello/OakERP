# Task Plan

## Task Name
ar-invoice-backend

## Goal
Implement the next backend-only AR invoice create/capture slice:
- add `POST /api/ar-invoices`
- create draft AR invoices through the existing API -> Application -> persistence seam
- persist the current AR invoice draft shape, including service and item lines
- validate request shape, master data, uniqueness, totals, and transactional draft persistence
- keep the slice aligned with the existing AR invoice posting flow and Swagger/API-doc standards

## Background
AR invoice persistence and posting already exist, but there is no Application/API capture slice for creating draft AR invoices. OakERP already supports AR invoice lines with `RevenueAccount`, `ItemId`, `LocationId`, and `TaxRateId`, and AR invoice posting already consumes that shape. This task fills the missing backend create seam without redesigning posting.

## Scope
- `OakERP.API`
- `OakERP.Application`
- `OakERP.Domain`
- `OakERP.Infrastructure`
- `OakERP.Tests.Unit`
- `OakERP.Tests.Integration`
- `docs/ai/tasks/active/ar-invoice-backend`

## Out of Scope
- frontend/UI/mobile/web flows
- bank transaction creation
- reversal or unposting
- generic posting transport endpoint redesign
- broad repository redesign
- architecture rewrites
- read/list/query/update/delete AR invoice endpoints
- posting-behavior redesign, FX expansion, or schema changes unless a hard blocker appears

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
1. Record the approved scope, current AR invoice shape, and known infrastructure gaps in the task docs.
2. Add the AR invoice Application/API slice, narrow repository helper, persistence-classifier support, and infrastructure wiring for item/location/tax validation.
3. Add Swagger example parity plus unit, integration, and Swagger coverage, then run targeted validation and record results.

## Validation Commands
```powershell
dotnet build OakERP.API/OakERP.API.csproj
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter FullyQualifiedName~ArInvoice
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ArInvoiceApiTests
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~SwaggerDocumentTests
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ArInvoicePostingTests
```

## Test Notes
This task requires both:
- unit tests for validator, workflow, snapshot, and result-path behavior
- integration tests for API wiring, persistence, rollback behavior, posting compatibility, and Swagger/example parity

## Risks

- `ITaxRateRepository` exists in Domain but currently has no Infrastructure implementation, so create-time tax-rate validation needs a small repo completion step.
- Item and location repositories exist in Infrastructure but are not currently registered in `AddRepositories()`.

## Architecture Checks

- Are runtime models and persisted entity models still cleanly separated?
- Were business-significant literals centralized instead of repeated?
- Do the tests exercise real fallback paths instead of pre-resolved values?
- Did any new abstraction clearly solve a real problem?
- Did thin orchestrators stay thin and pure engines stay pure where intended?

## Notes

- The command example must reflect the supported draft shape and include both a service line and an item line.
- No migration is expected for this slice.
- Targeted validation was sufficient for this coding pass; broader PR validation can be run later if the branch is prepared for review.
