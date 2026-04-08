# Progress

## Task
ap-payment-capture-allocation

## Started
2026-04-05 18:40:00

## Work Log
- Read `AGENTS.md`, architecture docs, workflow docs, and OakERP skills before editing.
- Used Serena for AP payment, AP invoice, repo, posting, and AR receipt pattern discovery.
- Created the AP payment task folder and recorded the approved scope/constraints.
- Added AP payment application contracts, result DTOs, and snapshot DTOs.
- Added `ApSettlementCalculator`, `ApPaymentCommandValidator`, `ApPaymentSnapshotFactory`, and `ApPaymentService`.
- Added narrow AP repository helpers for payment doc-number uniqueness, tracked payment allocation loads, and tracked AP invoice settlement loads.
- Registered AP payment repositories and services in Infrastructure DI.
- Added `ApPaymentsController` plus AP payment API route constants.
- Added AP payment unit tests and integration tests for the new slice.
- Resolved CSharpier formatting drift surfaced by `validate-pr` and reran validation.

## Files Touched
- `OakERP.Application/AccountsPayable/*`
- `OakERP.Domain/AccountsPayable/ApSettlementCalculator.cs`
- `OakERP.Domain/RepositoryInterfaces/AccountsPayable/IApPaymentRepository.cs`
- `OakERP.Domain/RepositoryInterfaces/AccountsPayable/IApInvoiceRepository.cs`
- `OakERP.Infrastructure/AccountsPayable/*`
- `OakERP.Infrastructure/Repositories/AccountsPayable/ApPaymentRepository.cs`
- `OakERP.Infrastructure/Repositories/AccountsPayable/ApInvoiceRepository.cs`
- `OakERP.Infrastructure/Extensions/ServiceCollectionExtensions.cs`
- `OakERP.API/Controllers/ApPaymentsController.cs`
- `OakERP.Tests.Unit/AccountsPayable/*`
- `OakERP.Tests.Integration/AccountsPayable/ApPaymentApiTests.cs`
- `OakERP.Tests.Integration/ApiRoutes.cs`
- `docs/ai/tasks/active/ap-payment-capture-allocation/*`

## Validation
- `dotnet build OakERP.API/OakERP.API.csproj`
  - Passed
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter FullyQualifiedName~AccountsPayable`
  - Passed
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ApPayment`
  - Passed
- `pwsh ./tools/validate-pr.ps1`
  - Passed
  - Initial run surfaced CSharpier formatting drift on the newly added AP payment files; formatting was normalized and the script passed on rerun.

## Remaining
- No in-scope work remains for the approved AP payment capture + allocation MVP.

## Deferred Smells / Risks
- AP payment posting was intentionally not added.
- `DiscountTaken`, `WriteOffAmount`, FX, bank transactions, and payment reversal behavior remain deferred.
- `ApPayment` still has no persisted currency field; this slice stays base-currency-only instead of widening into a payment-currency redesign.

## Outcome
- Backend-only `POST /api/ap-payments` and `POST /api/ap-payments/{paymentId}/allocations` now create and allocate draft AP payments transactionally with DTO-path validation failures.
- AP invoice settlement state now updates consistently from AP payment allocations without using `v_ap_open_items` as runtime truth.
