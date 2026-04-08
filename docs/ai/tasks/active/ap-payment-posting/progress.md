# Progress

## Task
ap-payment-posting

## Started
2026-04-05

## Work Log
- Re-read the required OakERP architecture, tasking, and workflow guidance.
- Activated Serena for symbol-aware discovery in the OakERP project.
- Audited the current AP payment capture/allocation implementation, shared posting runtime, and posting test structure.
- Created the active task docs for this slice.
- Added the AP payment runtime posting context and context-builder seam under `OakERP.Domain.Posting.AccountsPayable`.
- Added a narrow `IApPaymentRepository.GetTrackedForPostingAsync` load for posting without broadening repository scope.
- Extended the existing posting rule provider, posting engine, and posting service in place for `DocKind.ApPayment`.
- Kept AP payment posting GL-only, with zero inventory rows, zero bank transaction rows, and no AP payment capture redesign.
- Added unit coverage for AP payment posting engine behavior, context building, and posting-service orchestration.
- Added integration coverage for AP payment posting success, full-amount posting on partially allocated payments, double-post resistance, concurrent-post resistance, no-open-period rejection, non-base-currency rejection, and persisted over-allocation rejection.
- Normalized the touched files with CSharpier after the first `validate-pr` run reported formatting drift.

## Files Touched
- `docs/ai/tasks/active/ap-payment-posting/task_plan.md`
- `docs/ai/tasks/active/ap-payment-posting/findings.md`
- `docs/ai/tasks/active/ap-payment-posting/progress.md`
- `OakERP.Domain/Posting/IPostingEngine.cs`
- `OakERP.Domain/Posting/PostingSourceTypes.cs`
- `OakERP.Domain/Posting/AccountsPayable/ApPaymentPostingContext.cs`
- `OakERP.Domain/Posting/AccountsPayable/IApPaymentPostingContextBuilder.cs`
- `OakERP.Domain/RepositoryInterfaces/AccountsPayable/IApPaymentRepository.cs`
- `OakERP.Infrastructure/Extensions/ServiceCollectionExtensions.cs`
- `OakERP.Infrastructure/Repositories/AccountsPayable/ApPaymentRepository.cs`
- `OakERP.Infrastructure/Posting/PostingRuleProvider.cs`
- `OakERP.Infrastructure/Posting/PostingEngine.cs`
- `OakERP.Infrastructure/Posting/PostingService.cs`
- `OakERP.Infrastructure/Posting/AccountsPayable/ApPaymentPostingContextBuilder.cs`
- `OakERP.Tests.Unit/Posting/PostingServiceTestFactory.cs`
- `OakERP.Tests.Unit/Posting/PostingServiceTests.cs`
- `OakERP.Tests.Unit/Posting/ApPostingServiceTests.cs`
- `OakERP.Tests.Unit/Posting/PostingEngineApPaymentTests.cs`
- `OakERP.Tests.Unit/Posting/ApPaymentPostingContextBuilderTests.cs`
- `OakERP.Tests.Integration/Posting/ApPaymentPostingTests.cs`

## Validation
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter FullyQualifiedName~Posting`
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ApPaymentPosting`
- `dotnet build OakERP.API/OakERP.API.csproj`
- `pwsh ./tools/validate-pr.ps1`

## Validation Notes
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter FullyQualifiedName~Posting` passed.
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ApPaymentPosting` passed.
- `dotnet build OakERP.API/OakERP.API.csproj` passed.
- The first `pwsh ./tools/validate-pr.ps1` run surfaced formatting drift on touched files; formatting was normalized with `dotnet csharpier format`, and the rerun passed.

## Remaining
- None for this slice.

## Deferred Smells / Risks
- Bank transaction creation remains intentionally deferred.
- AP payment reversal/unposting remains intentionally deferred.
- Draft allocation timing continues to affect invoice settlement state before payment posting.
