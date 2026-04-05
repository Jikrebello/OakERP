# Progress

## Task
ap-invoice-posting

## Started
2026-04-05

## Work Log
- Re-read the required OakERP architecture, tasking, and workflow guidance.
- Activated Serena for symbol-aware discovery in the OakERP project.
- Audited the current AP invoice capture, posting seam, GL settings, and posting test structure.
- Created the active task docs for this slice.
- Added the AP invoice runtime posting context, line context, and context-builder seam under `OakERP.Domain.Posting.Accounts_Payable`.
- Added a narrow `IApInvoiceRepository.GetTrackedForPostingAsync` load for posting without broadening repository scope.
- Extended the existing posting rule provider, posting engine, and posting service in place for `DocKind.ApInvoice`.
- Kept AP invoice posting GL-only, with zero inventory rows, zero bank/cash rows, and no bank transaction creation.
- Added unit coverage for AP posting engine behavior, AP posting context building, and AP posting-service orchestration.
- Added integration coverage for AP invoice posting success, header-tax posting, double-post resistance, concurrent-post resistance, no-open-period rejection, non-base-currency rejection, and persisted item/tax-rate rejection.
- Normalized the touched files with CSharpier after the first `validate-pr` run reported formatting drift.

## Files Touched
- `docs/ai/tasks/active/ap-invoice-posting/task_plan.md`
- `docs/ai/tasks/active/ap-invoice-posting/findings.md`
- `docs/ai/tasks/active/ap-invoice-posting/progress.md`
- `OakERP.Domain/Posting/IPostingEngine.cs`
- `OakERP.Domain/Posting/PostingSourceTypes.cs`
- `OakERP.Domain/Posting/Accounts_Payable/ApInvoicePostingContext.cs`
- `OakERP.Domain/Posting/Accounts_Payable/ApInvoicePostingLineContext.cs`
- `OakERP.Domain/Posting/Accounts_Payable/IApInvoicePostingContextBuilder.cs`
- `OakERP.Domain/Repository_Interfaces/Accounts_Payable/IApInvoiceRepository.cs`
- `OakERP.Infrastructure/Extensions/ServiceCollectionExtensions.cs`
- `OakERP.Infrastructure/Repositories/Accounts_Payable/ApInvoiceRepository.cs`
- `OakERP.Infrastructure/Posting/PostingService.cs`
- `OakERP.Infrastructure/Posting/Accounts_Receivable/ArPostingEngine.cs`
- `OakERP.Infrastructure/Posting/Accounts_Receivable/ArPostingRuleProvider.cs`
- `OakERP.Infrastructure/Posting/Accounts_Payable/ApInvoicePostingContextBuilder.cs`
- `OakERP.Tests.Unit/Posting/PostingServiceTestFactory.cs`
- `OakERP.Tests.Unit/Posting/PostingServiceTests.cs`
- `OakERP.Tests.Unit/Posting/ApPostingEngineTests.cs`
- `OakERP.Tests.Unit/Posting/ApInvoicePostingContextBuilderTests.cs`
- `OakERP.Tests.Unit/Posting/ApPostingServiceTests.cs`
- `OakERP.Tests.Integration/Posting/ApInvoicePostingTests.cs`

## Validation
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter FullyQualifiedName~Posting`
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ApInvoicePosting`
- `dotnet build OakERP.API/OakERP.API.csproj`
- `pwsh ./tools/validate-pr.ps1`

## Validation Notes
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter FullyQualifiedName~Posting` passed.
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ApInvoicePosting` passed.
- `dotnet build OakERP.API/OakERP.API.csproj` passed.
- The first `pwsh ./tools/validate-pr.ps1` run surfaced formatting drift on the touched files; formatting was normalized with `dotnet csharpier format`, and the rerun passed.

## Remaining
- None for this slice.

## Deferred Smells / Risks
- The shared runtime implementation types remain AR-named (`ArPostingEngine`, `ArPostingRuleProvider`) even though they now support AP invoice posting.
- AP payment capture/allocation, bank/cash behavior, bank transaction creation, vendor credits, discounts, and write-offs remain intentionally deferred.
- Unposting/reversal, FX posting, inventory/landed-cost behavior, and persisted AP invoice posting-date behavior remain intentionally deferred.
