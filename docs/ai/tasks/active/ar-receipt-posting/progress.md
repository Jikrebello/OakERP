# Progress

## Task
ar-receipt-posting

## Started
2026-04-05

## Work Log
- Re-read the required OakERP architecture, tasking, and workflow guidance.
- Audited the current AR receipt, posting, GL, and bank seams.
- Confirmed Serena tooling was not available in this session.
- Created the active task docs for this slice.
- Added a runtime AR receipt posting context and context-builder seam without changing the application-facing posting contract.
- Added a narrow tracked receipt-posting repository load and extended the existing posting engine, posting rule provider, and posting service for `DocKind.ArReceipt`.
- Kept AR receipt posting GL-only, with zero inventory rows and no bank transaction creation.
- Added unit coverage for receipt posting engine behavior and posting-service orchestration/validation.
- Added integration coverage for unapplied posting, partially allocated posting, double-post resistance, concurrent-post resistance, no-open-period rejection, non-base-currency rejection, and over-allocation rejection.
- Normalized the touched C# files with CSharpier after `validate-pr` surfaced formatting drift.

## Files Touched
- `docs/ai/tasks/active/ar-receipt-posting/task_plan.md`
- `docs/ai/tasks/active/ar-receipt-posting/findings.md`
- `docs/ai/tasks/active/ar-receipt-posting/progress.md`
- `OakERP.Domain/Posting/IPostingEngine.cs`
- `OakERP.Domain/Posting/PostingSourceTypes.cs`
- `OakERP.Domain/Posting/AccountsReceivable/ArReceiptPostingContext.cs`
- `OakERP.Domain/Posting/AccountsReceivable/IArReceiptPostingContextBuilder.cs`
- `OakERP.Domain/RepositoryInterfaces/AccountsReceivable/IArReceiptRepository.cs`
- `OakERP.Infrastructure/Extensions/ServiceCollectionExtensions.cs`
- `OakERP.Infrastructure/Repositories/AccountsReceivable/ArReceiptRepository.cs`
- `OakERP.Infrastructure/Posting/PostingService.cs`
- `OakERP.Infrastructure/Posting/AccountsReceivable/ArInvoicePostingEngine.cs`
- `OakERP.Infrastructure/Posting/AccountsReceivable/ArInvoicePostingRuleProvider.cs`
- `OakERP.Infrastructure/Posting/AccountsReceivable/ArReceiptPostingContextBuilder.cs`
- `OakERP.Tests.Unit/Posting/PostingServiceTestFactory.cs`
- `OakERP.Tests.Unit/Posting/PostingServiceTests.cs`
- `OakERP.Tests.Unit/Posting/ArReceiptPostingEngineTests.cs`
- `OakERP.Tests.Integration/Posting/ArReceiptPostingTests.cs`

## Validation
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter FullyQualifiedName~Posting`
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ArReceiptPosting`
- `dotnet build OakERP.sln`
- `pwsh ./tools/validate-pr.ps1`

## Validation Notes
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter FullyQualifiedName~Posting` passed.
- The first targeted integration-test run hit a transient file lock on `OakERP.Common\obj\Debug\net9.0\OakERP.Common.dll`; the immediate retry passed without code changes.
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ArReceiptPosting` passed.
- `dotnet build OakERP.sln` failed outside this slice because MAUI workloads are not installed locally:
  - `maui-tizen` required by `OakERP.Desktop`
  - `maui-tizen` and `wasm-tools` required by `OakERP.Mobile`
- `pwsh ./tools/validate-pr.ps1` passed after formatting the touched C# files with CSharpier.

## Remaining
- None for this slice.

## Deferred Smells / Risks
- Bank transaction creation remains intentionally deferred.
- Unposting/reversal remains intentionally deferred.
- The existing posting seam remains exception-based for posting failures; this slice should not broaden into a posting contract redesign.
- FX, discount, write-off, dedicated unapplied-cash liability, and posting transport endpoints remain intentionally deferred.
