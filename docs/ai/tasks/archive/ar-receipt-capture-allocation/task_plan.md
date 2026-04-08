# Task Plan

## Task Name
ar-receipt-capture-allocation

## Goal
Implement the smallest safe backend-first AR receipt capture + allocation slice from API to DB, without UI work and without introducing AR receipt posting.

## Background
OakERP already has persisted AR receipt and receipt-allocation entities, repositories, and EF mappings, but it does not yet have an application/API slice for receipt capture or allocation. AR invoice posting already exists and should remain separate from this work.

## Scope
- `OakERP.API`
- `OakERP.Application`
- `OakERP.Domain`
- `OakERP.Infrastructure`
- `OakERP.Tests.Unit`
- `OakERP.Tests.Integration`
- `docs/ai/tasks/active/ar-receipt-capture-allocation/`

## Out of Scope
- UI, screen flows, or frontend validation
- read/list/query endpoints
- receipt posting, GL entries, bank transactions, or fiscal-period posting checks
- AR posting engine redesign
- discount/write-off allocation behavior
- using `v_ar_open_items` as the source of truth for allocation math
- repository redesign beyond narrow entity-local additions needed for this slice

## Constraints
- Keep receipts in `Draft` state only.
- Keep service orchestration thin and keep business rules out of controllers.
- Keep expected business failures on the DTO/result path rather than exception-driven flow.
- Preserve future ability to add AR receipt posting in a later slice.
- Stop and report if the repository additions start forcing a broader redesign.

## Success Criteria
- [x] A receipt can be created through the backend API.
- [x] A receipt can be created without allocations.
- [x] A receipt can be created with one or more allocations in the same request.
- [x] Additional allocations can be added to an existing draft receipt.
- [x] Customer/invoice consistency is validated.
- [x] Over-allocation is prevented at both receipt and invoice level.
- [x] Invoice status/remaining behavior updates correctly without using the SQL view as source of truth.
- [x] Receipts remain draft-only.
- [x] Unit and integration tests cover the new business behavior.
- [x] Task docs record findings, validation, and deferred risks.

## Planned Steps
1. Create task docs and record the current repository state for receipts, invoices, and repositories.
2. Add the minimal application contracts and result DTOs for receipt creation and allocation.
3. Add narrow repository methods and infrastructure service implementation for tracked receipt/invoice allocation work.
4. Wire the new receipt service into DI and expose backend-only API endpoints for create and allocate.
5. Add unit tests for allocation rules and orchestration behavior.
6. Add integration tests for API-to-DB behavior, including transactional failure cases.
7. Run targeted validation first, then broader validation if needed, and record results in `progress.md`.

## Validation Commands
```powershell
dotnet build OakERP.API/OakERP.API.csproj
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter ArReceipt
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter ArReceipt
pwsh ./tools/validate-pr.ps1
```

## Test Notes
- Add unit tests for local receipt/allocation business rules and service behavior.
- Add integration tests for runtime wiring, persistence, API behavior, and transactional rollback behavior.

## Risks
- The current API has no existing finance CRUD result pattern outside auth, so the new result DTOs must stay explicit and predictable.
- AR invoice state does not currently persist a remaining/open amount; the service must derive settlement state from tracked invoice totals and allocation rows.
- Receipt FX fields already exist on the entity, but invoice posting is currently base-currency-only; the slice must avoid locking in incorrect FX allocation semantics.

## Architecture Checks
- No UI/shared boundary changes.
- No posting-engine redesign.
- No new migration expected unless current repository constraints prove insufficient.
- Any new repository method must stay narrow and entity-local.
