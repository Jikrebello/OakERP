# Task Plan

## Task Name
ap-payment-capture-allocation

## Goal
Implement the smallest safe backend-only AP payment capture + allocation MVP:
- add `POST /api/ap-payments`
- add `POST /api/ap-payments/{paymentId}/allocations`
- create draft AP payments with optional initial allocations
- validate vendor, bank account, document uniqueness, base-currency-only scope, invoice/vendor consistency, and allocation limits
- persist payment, allocations, and AP invoice status changes transactionally
- add unit and integration tests in the same slice

## Background
OakERP is being built backend-first. AR invoice posting, AR receipt capture/allocation, AR receipt posting, AP invoice capture, and AP invoice posting already exist. AP payment entities and persistence already exist, but there is no AP payment application service, API endpoint, DI registration, or tests for draft payment capture/allocation.

## Scope
- `OakERP.Application/AccountsPayable`
- `OakERP.Domain/Accounts_Payable`
- `OakERP.Domain/Repository_Interfaces/Accounts_Payable`
- `OakERP.Infrastructure/Accounts_Payable`
- `OakERP.Infrastructure/Repositories/Accounts_Payable`
- `OakERP.Infrastructure/Extensions`
- `OakERP.API/Controllers`
- `OakERP.Tests.Unit/AccountsPayable`
- `OakERP.Tests.Integration/AccountsPayable`
- `OakERP.Tests.Integration/ApiRoutes.cs`
- `docs/ai/tasks/active/ap-payment-capture-allocation`

## Out of Scope
- AP payment posting
- AP payment reversal/unposting/reallocation/removal
- AP invoice capture/posting redesign
- read/list/query endpoints
- edit/update/delete endpoints
- UI or screen-flow work
- schema changes unless a hard blocker appears
- FX, discounts, write-offs, or bank transaction behavior

## Constraints
- Preserve behavior unless explicitly asked otherwise.
- Keep the change set as small as possible.
- Keep payments `Draft` only in this slice.
- Keep expected business failures on the DTO/result path.
- Do not use `v_ap_open_items` for allocation math.
- Keep repository additions entity-local and narrow.

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
1. Add AP payment application contracts, domain settlement helper, narrow repository helpers, and Infrastructure validator/snapshot/service seams.
2. Add the AP payment controller, DI wiring, and AP payment unit/integration coverage.
3. Run the approved validation commands and update task docs with outcomes, transaction notes, and deferrals.

## Validation Commands
```powershell
dotnet build OakERP.API/OakERP.API.csproj
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter FullyQualifiedName~AccountsPayable
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ApPayment
pwsh ./tools/validate-pr.ps1
```

## Risks
- `ApPayment` has no persisted currency field, so the slice must stay base-currency-only and derive the effective payment currency from the selected bank account.
- `DiscountTaken` and `WriteOffAmount` already exist on persisted allocation rows and must remain explicitly deferred in request handling.
- This slice mirrors AR receipt behavior by allowing draft payment allocation to affect AP invoice settlement state before payment posting.

## Notes
- Serena was available in this session and used for symbol-aware discovery before implementation.
- No schema change was required for this slice.
