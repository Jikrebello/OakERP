# Progress

## Task
ar-receipt-capture-allocation

## Started
2026-04-05

## Work Log
- Re-read the required OakERP architecture and workflow guidance.
- Audited the current AR invoice, AR receipt, allocation, repository, API, and test-harness state.
- Confirmed Serena tooling was not available in this session.
- Created the active task folder and initial task docs for this slice.
- Added AR receipt application contracts, commands, allocation input models, receipt/invoice snapshot DTOs, and a result DTO that stays on the existing DTO/result path for expected business failures.
- Added narrow repository methods for receipt duplicate detection and tracked allocation loads, without widening the repository pattern beyond this slice.
- Implemented `ArReceiptService` in Infrastructure for draft-only receipt creation and allocation, with customer/bank/invoice/currency/over-allocation validation and invoice settlement-state updates.
- Registered the receipt, allocation, customer, and bank repositories in DI and added AR receipt service registration.
- Added backend-only AR receipt API endpoints for create and allocate, keeping controllers thin and leaving business rules in the service.
- Added unit tests for create, allocate, over-allocation, customer mismatch, duplicate doc number, and concurrency-result behavior.
- Added integration tests for create-unapplied, create-with-multiple-allocations, later allocation, customer mismatch rollback, over-allocation rollback, and non-base-currency rejection.
- Resolved an integration-only EF concurrency issue by adding new allocation rows through the existing allocation repository and building response snapshots from explicit service-side allocation state instead of direct tracked-collection mutation on existing receipts.

## Files Touched
- `docs/ai/tasks/active/ar-receipt-capture-allocation/task_plan.md`
- `docs/ai/tasks/active/ar-receipt-capture-allocation/findings.md`
- `docs/ai/tasks/active/ar-receipt-capture-allocation/progress.md`
- `OakERP.API/Controllers/ArReceiptsController.cs`
- `OakERP.API/OakERP.API.csproj`
- `OakERP.API/Program.cs`
- `OakERP.Application/AccountsReceivable/AllocateArReceiptCommand.cs`
- `OakERP.Application/AccountsReceivable/ArInvoiceSettlementSnapshotDTO.cs`
- `OakERP.Application/AccountsReceivable/ArReceiptAllocationInputDTO.cs`
- `OakERP.Application/AccountsReceivable/ArReceiptAllocationSnapshotDTO.cs`
- `OakERP.Application/AccountsReceivable/ArReceiptCommandResultDTO.cs`
- `OakERP.Application/AccountsReceivable/ArReceiptSnapshotDTO.cs`
- `OakERP.Application/AccountsReceivable/CreateArReceiptCommand.cs`
- `OakERP.Application/AccountsReceivable/IArReceiptService.cs`
- `OakERP.Domain/RepositoryInterfaces/AccountsReceivable/IArInvoiceRepository.cs`
- `OakERP.Domain/RepositoryInterfaces/AccountsReceivable/IArReceiptRepository.cs`
- `OakERP.Infrastructure/AccountsReceivable/ArReceiptService.cs`
- `OakERP.Infrastructure/Extensions/ServiceCollectionExtensions.cs`
- `OakERP.Infrastructure/Repositories/AccountsReceivable/ArInvoiceRepository.cs`
- `OakERP.Infrastructure/Repositories/AccountsReceivable/ArReceiptRepository.cs`
- `OakERP.Tests.Integration/AccountsReceivable/ArReceiptApiTests.cs`
- `OakERP.Tests.Integration/ApiRoutes.cs`
- `OakERP.Tests.Unit/AccountsReceivable/ArReceiptServiceTestFactory.cs`
- `OakERP.Tests.Unit/AccountsReceivable/ArReceiptServiceTests.cs`

## Validation
- `dotnet build OakERP.API/OakERP.API.csproj`
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter ArReceipt`
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter ArReceipt`

## Validation Notes
- All targeted AR receipt validation passed.
- `OakERP.API` still has a pre-existing warning at `OakERP.API/Controllers/BaseController.cs(23,57)` (`CS8629`). This slice did not change that controller.
- `pwsh ./tools/validate-pr.ps1` was not run because the targeted project build and targeted unit/integration suites fully covered the new receipt slice behavior.

## Remaining
- None for this slice.

## Deferred Smells / Risks
- Repository scope must stay narrow; if a broader redesign becomes necessary, stop and report instead of widening the slice.
- Foreign-currency allocation behavior remains deferred.
- Receipt posting, GL entries, bank transactions, fiscal-period posting checks, discounts, write-offs, and read/list/query endpoints remain deferred to later slices.
