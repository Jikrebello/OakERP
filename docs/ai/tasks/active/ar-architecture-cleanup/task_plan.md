# Task Plan

## Task Name
ar-architecture-cleanup

## Goal
Refactor the current AR backend slice to improve seam clarity, reduce duplicated settlement logic, and make receipt orchestration thinner without changing behavior, schema, or transport flow.

## Background
AR receipt capture/allocation and AR receipt posting were both working, but the current implementation had three cleanup pressures:
- receipt settlement math was duplicated across orchestration and posting
- receipt DTO/result mapping and command validation were bundled into `ArReceiptService`
- shared AR posting behavior lived in invoice-named infrastructure types

## Scope
- `OakERP.Domain`
- `OakERP.Infrastructure`
- `OakERP.Tests.Unit`
- `docs/ai/tasks/active/ar-architecture-cleanup/`

## Out of Scope
- posting result-path redesign
- Domain package/dependency cleanup outside the AR-local slice
- schema or migration changes
- API transport changes
- runtime-vs-persisted posting-rule family rename

## Constraints
- Preserve current AR behavior.
- Keep backend-only seams unchanged.
- Keep repositories narrow and entity-local.
- Do not add schema changes or posting controller work.

## Success Criteria
- [x] Shared AR settlement math lives in one Domain helper
- [x] `ArReceiptService` delegates command validation and snapshot mapping
- [x] Shared AR posting engine/provider names match current ownership
- [x] Relevant backend build/test validation passes
- [x] Deferred structural work is documented explicitly

## Planned Steps
1. Extract shared AR settlement calculations into a Domain helper and reuse them in receipt orchestration and receipt posting.
2. Split receipt command validation and receipt snapshot mapping out of `ArReceiptService` into small Infrastructure-local collaborators.
3. Rename the AR posting engine/provider to reflect that they now own both invoice and receipt posting behavior.
4. Add unit coverage for the extracted components and rerun AR-focused validation.
5. Record completed work and deferred follow-up in task docs.

## Validation Commands
```powershell
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter "FullyQualifiedName~AccountsReceivable|FullyQualifiedName~Posting"
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter "FullyQualifiedName~ArReceipt|FullyQualifiedName~ArInvoicePosting"
dotnet build OakERP.API/OakERP.API.csproj
pwsh ./tools/validate-pr.ps1
```

## Test Notes
This task requires:
- unit tests for extracted collaborators and calculators
- integration validation to confirm wiring remained stable

## Risks
- `PostingService` still uses exception-based failures for expected posting errors.
- The Domain project still carries a framework package dependency unrelated to this AR-local slice.

## Architecture Checks
- Runtime and persisted posting-rule families were left unchanged and explicitly deferred.
- Shared settlement math is now centralized.
- The receipt controller remains thin.
- The receipt service remains the orchestration owner, but validation and DTO shaping were pulled out.

## Notes
- No schema or migration changes were made.
- Receipt posting output remains GL-only with zero inventory rows.
