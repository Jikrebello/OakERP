# Task Plan

## Task Name
ar-invoice-posting-1a

## Goal
Implement the first executable AR invoice posting slice through the existing application posting seam, limited to base-currency GL posting for AR control, revenue, and tax output only.

## Background
OakERP already contains application posting commands and domain posting abstractions, but no working backend posting path. This slice should wire the first narrow vertical path without broadening into inventory, FX, unposting, or schema redesign.

## Scope
- `OakERP.Domain` posting contracts and repository interfaces only where a narrow posting query method is required
- `OakERP.Infrastructure` repository implementations, posting services, and DI registration
- `OakERP.Tests.Unit`
- `OakERP.Tests.Integration`
- `docs/ai/tasks/active/ar-invoice-posting-1a/`

## Out of Scope
- Inventory movements
- COGS and inventory asset postings
- FX conversion support beyond base-currency rejection
- Unposting support
- Schema changes, including `LocationId` work
- Changes to `OakERP.Domain.Entities.Posting`
- API endpoints or UI work

## Constraints
- Preserve behavior unless explicitly asked otherwise.
- Do not introduce secrets or hardcoded environment values.
- Keep the change set as small as possible.
- Prefer structural improvement over cosmetic churn.
- Keep `PostingService` thin and orchestration-only.
- Keep `ArInvoicePostingEngine` pure and deterministic.
- Do not let `PostingService` depend directly on `ApplicationDbContext`.
- If a required repository addition widens responsibility beyond this slice, stop and report the conflict.

## Success Criteria
- [x] `IPostingService.PostAsync` supports `DocKind.ArInvoice`
- [x] Base-currency AR invoice GL posting writes AR control, revenue, and tax entries only
- [x] Posted invoice is marked `Posted` and assigned posting date transactionally
- [x] Double-post attempts do not create duplicate GL rows
- [x] Stock-line invoices are rejected explicitly for this slice
- [x] Required unit tests are added and pass
- [x] Required integration tests are added and pass
- [x] Task docs reflect findings, progress, and validation

## Planned Steps
1. Update task docs with approved slice boundaries and current-state findings.
2. Add narrow repository methods for posting-safe AR invoice and fiscal period loading if they remain within current repository ownership.
3. Implement Infrastructure posting services: thin `PostingService`, pure `ArInvoicePostingEngine`, code-backed AR invoice rule provider, and GL settings provider.
4. Add unit tests for engine and orchestration behavior.
5. Add integration tests for successful posting, transactional failures, and double-post resistance.
6. Run validation in the approved order and update progress notes.

## Validation Commands
```powershell
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ArInvoicePosting
dotnet build OakERP.sln
```

## Test Notes
State whether this task requires:
- unit tests
- integration tests
- both
- neither (with reason)

This task requires both unit and integration tests in the same slice.

## Risks

- Existing repository contracts may not expose enough posting-safe read shape without drifting into broader refactor work.
- There are duplicated posting rule types in the repo; this slice must use only the runtime `OakERP.Domain.Posting` types.

## Notes

- Integration tests should resolve `IPostingService` from the existing test host rather than introducing an HTTP endpoint.
- `Force` and `UnpostAsync` should be rejected or left unsupported within this slice.
- Repository additions were kept narrow and entity-local:
  - `IArInvoiceRepository.GetTrackedForPostingAsync`
  - `IFiscalPeriodRepository.GetOpenForDateAsync`
