# Progress

## Task
ap-invoice-backend

## Started
2026-04-05 17:19:00

## Work Log
- Read `AGENTS.md`, architecture docs, workflow docs, and OakERP skills before editing.
- Confirmed Serena was not available in this session.
- Created the AP invoice task folder with the repo task scaffold.
- Added AP invoice application contracts, result DTOs, and snapshot DTOs.
- Added `ApInvoiceCommandValidator`, `ApInvoiceSnapshotFactory`, and `ApInvoiceService`.
- Added narrow AP invoice repository helpers for `DocNo` and `(VendorId, InvoiceNo)` uniqueness checks.
- Registered AP and currency repositories plus AP invoice services in Infrastructure DI.
- Added `ApInvoicesController` and wired it into the existing API composition path.
- Added unit tests and integration tests for the new slice.
- Resolved CSharpier formatting drift surfaced by `validate-pr` and reran validation.

## Files Touched
- `OakERP.Application/AccountsPayable/*`
- `OakERP.Domain/Repository_Interfaces/Accounts_Payable/IApInvoiceRepository.cs`
- `OakERP.Infrastructure/Accounts_Payable/*`
- `OakERP.Infrastructure/Repositories/Accounts_Payable/ApInvoiceRepository.cs`
- `OakERP.Infrastructure/Extensions/ServiceCollectionExtensions.cs`
- `OakERP.API/Controllers/ApInvoicesController.cs`
- `OakERP.API/Program.cs`
- `OakERP.Tests.Unit/AccountsPayable/*`
- `OakERP.Tests.Integration/AccountsPayable/ApInvoiceApiTests.cs`
- `OakERP.Tests.Integration/ApiRoutes.cs`
- `docs/ai/tasks/active/ap-invoice-backend/*`

## Validation
- `dotnet build OakERP.API/OakERP.API.csproj`
  - Passed
  - Pre-existing warning remains in `OakERP.API/Controllers/BaseController.cs` (`CS8629`)
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter FullyQualifiedName~AccountsPayable`
  - Passed
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ApInvoice`
  - Passed
- `pwsh ./tools/validate-pr.ps1`
  - Passed after formatting the affected files with CSharpier

## Remaining
- No in-scope work remains for the approved AP invoice backend MVP.

## Deferred Smells / Risks
- AP invoice posting was intentionally not added.
- AP payment, allocation, discount-taken, write-off, and bank-impact behavior remain deferred.
- Item-based/inventory-style AP invoice behavior remains deferred and is rejected in v1.
- Tax-rate support remains deferred and is rejected in v1.

## Outcome
- Backend-only `POST /api/ap-invoices` now creates draft AP invoices transactionally with DTO-path validation failures.
- The slice stays aligned with the existing repository/unit-of-work pattern and does not broaden into posting or payments.

## Next Recommended Step
- Implement AP invoice posting as the next backend slice, keeping the app-facing posting seam unchanged and deferring AP payment behavior until after invoice posting is stable.
