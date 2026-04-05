# Progress

## Task
posting-runtime-normalization

## Started
2026-04-05

## Work Log
- Re-read the required OakERP architecture, tasking, and workflow guidance.
- Activated Serena and traced the shared posting runtime references.
- Confirmed the shared concrete runtime is referenced directly only by DI and posting unit tests.
- Created the active task docs for this cleanup slice.
- Renamed and re-homed `ArPostingEngine` to `PostingEngine` under `OakERP.Infrastructure.Posting`.
- Renamed and re-homed `ArPostingRuleProvider` to `PostingRuleProvider` under `OakERP.Infrastructure.Posting`.
- Updated DI registration to resolve `IPostingEngine` and `IPostingRuleProvider` through the normalized shared concrete types.
- Updated direct posting-engine unit tests and their file/class names to reference the shared runtime names.
- Normalized the shared inventory-validation text in `PostingService` so it no longer claims AR-invoice-specific behavior.
- Normalized the touched files with CSharpier after `validate-pr` reported line-ending formatting drift on a subset of files.

## Files Touched
- `docs/ai/tasks/active/posting-runtime-normalization/task_plan.md`
- `docs/ai/tasks/active/posting-runtime-normalization/findings.md`
- `docs/ai/tasks/active/posting-runtime-normalization/progress.md`
- `OakERP.Infrastructure/Posting/PostingEngine.cs`
- `OakERP.Infrastructure/Posting/PostingRuleProvider.cs`
- `OakERP.Infrastructure/Posting/PostingService.cs`
- `OakERP.Infrastructure/Extensions/ServiceCollectionExtensions.cs`
- `OakERP.Tests.Unit/Posting/PostingEngineArInvoiceTests.cs`
- `OakERP.Tests.Unit/Posting/PostingEngineArReceiptTests.cs`
- `OakERP.Tests.Unit/Posting/PostingEngineApInvoiceTests.cs`

## Validation
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter FullyQualifiedName~Posting`
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ArInvoicePosting`
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ArReceiptPosting`
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ApInvoicePosting`
- `dotnet build OakERP.API/OakERP.API.csproj`
- `pwsh ./tools/validate-pr.ps1`

## Validation Notes
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter FullyQualifiedName~Posting` passed.
- The first attempt to run the three posting integration suites in parallel hit a transient `OakERP.API.dll` file lock from a local `.NET Host` process; rerunning the AR invoice and AR receipt suites sequentially passed without code changes.
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ArInvoicePosting` passed.
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ArReceiptPosting` passed.
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ApInvoicePosting` passed.
- `dotnet build OakERP.API/OakERP.API.csproj` passed.
- The first `pwsh ./tools/validate-pr.ps1` run reported formatting drift on three touched files; after `dotnet csharpier format`, the rerun passed.

## Remaining
- None for this slice.

## Deferred Smells / Risks
- `IPostingEngine` and `IPostingRuleProvider` remain document-method-based and were intentionally not redesigned in this cleanup.
- `PostingService` remains a single orchestrator and was intentionally not decomposed into per-document handlers.
- Runtime-vs-persisted `PostingRule` family clarification remains deferred.
- No new posting document types, posting entrypoints, or reversal/unposting behavior were added in this slice.
