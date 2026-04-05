# Progress

## Task
ar-architecture-cleanup

## Started
2026-04-05 16:45:16

## Work Log
- Created the task folder and audited the current AR structure across Domain, Application, Infrastructure, API, and tests.
- Added `ArSettlementCalculator` in Domain and reused it across AR receipt orchestration and posting.
- Added `ArReceiptCommandValidator` and `ArReceiptSnapshotFactory` in Infrastructure and updated `ArReceiptService` to use them.
- Renamed the shared AR posting engine/provider from invoice-specific names to `ArPostingEngine` and `ArPostingRuleProvider`.
- Trimmed unused fields from `ArReceiptPostingContext`.
- Added unit tests for the extracted calculator and collaborators.

## Files Touched
- `OakERP.Domain/Accounts_Receivable/ArSettlementCalculator.cs`
- `OakERP.Domain/Posting/Accounts_Receivable/ArReceiptPostingContext.cs`
- `OakERP.Infrastructure/Accounts_Receivable/ArReceiptCommandValidator.cs`
- `OakERP.Infrastructure/Accounts_Receivable/ArReceiptSnapshotFactory.cs`
- `OakERP.Infrastructure/Accounts_Receivable/ArReceiptService.cs`
- `OakERP.Infrastructure/Extensions/ServiceCollectionExtensions.cs`
- `OakERP.Infrastructure/Posting/Accounts_Receivable/ArPostingEngine.cs`
- `OakERP.Infrastructure/Posting/Accounts_Receivable/ArPostingRuleProvider.cs`
- `OakERP.Infrastructure/Posting/Accounts_Receivable/ArReceiptPostingContextBuilder.cs`
- `OakERP.Infrastructure/Posting/PostingService.cs`
- `OakERP.Tests.Unit/AccountsReceivable/*`
- `OakERP.Tests.Unit/Posting/*`

## Validation
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter "FullyQualifiedName~AccountsReceivable|FullyQualifiedName~Posting"` passed
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter "FullyQualifiedName~ArReceipt|FullyQualifiedName~ArInvoicePosting"` passed
- `dotnet build OakERP.API/OakERP.API.csproj` passed
- `pwsh ./tools/validate-pr.ps1` passed

## Remaining
- No in-scope implementation work remains for this cleanup slice.

## Deferred Smells / Risks
- Posting still uses exception-path failures for expected business conditions.
- `OakERP.Domain` still carries an Identity EF Core package reference.
- Runtime and persisted posting-rule families still have overlapping names.

## Outcome
- AR settlement rules are centralized.
- Receipt orchestration is thinner and easier to follow.
- Shared AR posting infrastructure names now match actual ownership.
- Behavior, schema, and transport seams were preserved.

## Next Recommended Step
- Choose one separate follow-up slice:
  - posting result DTO standardization
  - Domain dependency cleanup
  - posting-rule family naming/documentation cleanup
