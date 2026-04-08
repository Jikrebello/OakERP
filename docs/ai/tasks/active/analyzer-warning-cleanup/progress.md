# Progress

## Task
analyzer-warning-cleanup

## Started
2026-04-08 13:59:50

## Work Log
- Removed commented-out cookie-auth code from `AuthService` and replaced the 9-parameter audit logger helper with an `AuditContext` record.
- Renamed `CurrencyISOCodes` to `CurrencyIsoCodes`, updated the matching file name, and updated all default currency references.
- Made AP/AR validator and snapshot helper classes explicitly `static`.
- Removed injected static helpers from service constructors and updated unit test factories accordingly.
- Added small dependency bundles for `ApPaymentService`, `ArReceiptService`, and `PostingService` to reduce constructor fan-in without changing behavior.
- Split `ApInvoiceCommandValidator.ValidateRequest(...)` into focused validation helpers.
- Split `PostingService.ValidatePostingResult(...)` into GL-entry and inventory validation helpers.
- Fixed catch-clause logging to pass exceptions and deconstructed tuple-return call sites in AP/AR services.

## Files Touched
- `OakERP.Auth/AuthService.cs`
- `OakERP.Common/Enums/CurrencyIsoCodes.cs`
- `OakERP.Domain/Entities/Accounts_Payable/ApInvoice.cs`
- `OakERP.Domain/Entities/Accounts_Receivable/ArInvoice.cs`
- `OakERP.Domain/Entities/Accounts_Receivable/ArReceipt.cs`
- `OakERP.Domain/Entities/Bank/BankAccount.cs`
- `OakERP.Infrastructure/Accounts_Payable/ApInvoiceCommandValidator.cs`
- `OakERP.Infrastructure/Accounts_Payable/ApInvoiceService.cs`
- `OakERP.Infrastructure/Accounts_Payable/ApInvoiceSnapshotFactory.cs`
- `OakERP.Infrastructure/Accounts_Payable/ApPaymentCommandValidator.cs`
- `OakERP.Infrastructure/Accounts_Payable/ApPaymentService.cs`
- `OakERP.Infrastructure/Accounts_Payable/ApPaymentSnapshotFactory.cs`
- `OakERP.Infrastructure/Accounts_Payable/ApPaymentServiceDependencies.cs`
- `OakERP.Infrastructure/Accounts_Receivable/ArReceiptCommandValidator.cs`
- `OakERP.Infrastructure/Accounts_Receivable/ArReceiptService.cs`
- `OakERP.Infrastructure/Accounts_Receivable/ArReceiptSnapshotFactory.cs`
- `OakERP.Infrastructure/Accounts_Receivable/ArReceiptServiceDependencies.cs`
- `OakERP.Infrastructure/Posting/PostingEngine.cs`
- `OakERP.Infrastructure/Posting/PostingService.cs`
- `OakERP.Infrastructure/Posting/PostingServiceDependencies.cs`
- `OakERP.Infrastructure/Extensions/ServiceCollectionExtensions.cs`
- `OakERP.Tests.Unit/AccountsPayable/ApInvoiceCommandValidatorTests.cs`
- `OakERP.Tests.Unit/AccountsPayable/ApInvoiceServiceTestFactory.cs`
- `OakERP.Tests.Unit/AccountsPayable/ApPaymentServiceTestFactory.cs`
- `OakERP.Tests.Unit/AccountsReceivable/ArReceiptCommandValidatorTests.cs`
- `OakERP.Tests.Unit/AccountsReceivable/ArReceiptServiceTestFactory.cs`
- `OakERP.Tests.Unit/AccountsReceivable/ArReceiptSnapshotFactoryTests.cs`
- `OakERP.Tests.Unit/Posting/PostingServiceTestFactory.cs`

## Validation
- `dotnet build OakERP.Auth/OakERP.Auth.csproj` succeeded
- `dotnet build OakERP.Infrastructure/OakERP.Infrastructure.csproj` succeeded
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj` succeeded: 80 passed, 0 failed
- `dotnet build OakERP.sln` succeeded with 0 warnings and 0 errors
- `dotnet format analyzers OakERP.Auth/OakERP.Auth.csproj --no-restore --verify-no-changes --severity info` succeeded
- `dotnet format analyzers OakERP.Common/OakERP.Common.csproj --no-restore --verify-no-changes --severity info` succeeded
- `dotnet format analyzers OakERP.Infrastructure/OakERP.Infrastructure.csproj --no-restore --verify-no-changes --severity info` now only reports unrelated `CA1861` suggestions in generated migration `20251201170038_Init.cs`

## Remaining
- No remaining issues in the user-reported warning set.
- Generated migration analyzer suggestions can be handled in a separate pass if desired.

## Deferred Smells / Risks
- Generated migration `20251201170038_Init.cs` still carries `CA1861` info-level suggestions; this task intentionally avoided churning generated migration code.

## Outcome
- The reported Auth/Common/Infrastructure diagnostics were resolved with behavior-preserving refactors and constructor cleanup.

## Next Recommended Step
- Send the next warning batch, or if you want a stricter cleanup pass, target the generated migration analyzer suggestions separately.
