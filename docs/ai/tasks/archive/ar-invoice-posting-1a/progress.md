# Progress

## Task
ar-invoice-posting-1a

## Started
2026-03-29 16:14:53

## Work Log
- Created the task scaffold with `tools/new-codex-task.ps1`.
- Recorded approved scope, current-state findings, and validation plan before starting code changes.
- Added narrow repository query methods for AR invoice posting loads and open fiscal period lookup.
- Implemented thin posting orchestration in Infrastructure through `IPostingService` and `IUnitOfWork`.
- Implemented a pure deterministic AR invoice GL posting engine and a code-backed runtime rule provider.
- Added an `app_settings`-backed GL settings provider.
- Registered posting services and required repositories in Infrastructure DI and API composition.
- Added unit tests for engine output and orchestration behavior.
- Added integration tests for happy path, transactional failures, stock-line rejection, non-base-currency rejection, and double-post resistance.
- Fixed the integration test host/helpers to use async DI scopes so async-disposable services are disposed correctly during posting tests.

## Files Touched
- `docs/ai/tasks/active/ar-invoice-posting-1a/task_plan.md`
- `docs/ai/tasks/active/ar-invoice-posting-1a/findings.md`
- `docs/ai/tasks/active/ar-invoice-posting-1a/progress.md`
- `OakERP.Domain/Posting/AccountsReceivable/ArInvoicePostingContext.cs`
- `OakERP.Domain/RepositoryInterfaces/AccountsReceivable/IArInvoiceRepository.cs`
- `OakERP.Domain/RepositoryInterfaces/GeneralLedger/IFiscalPeriodRepository.cs`
- `OakERP.Infrastructure/Extensions/ServiceCollectionExtensions.cs`
- `OakERP.Infrastructure/Persistence/DesignTimeDbContextFactory.cs`
- `OakERP.Infrastructure/Posting/PostingService.cs`
- `OakERP.Infrastructure/Posting/AccountsReceivable/ArInvoicePostingEngine.cs`
- `OakERP.Infrastructure/Posting/AccountsReceivable/ArInvoicePostingRuleProvider.cs`
- `OakERP.Infrastructure/Posting/GeneralLedger/AppSettingGlSettingsProvider.cs`
- `OakERP.Infrastructure/Repositories/AccountsReceivable/ArInvoiceRepository.cs`
- `OakERP.Infrastructure/Repositories/GeneralLedger/FiscalPeriodRepository.cs`
- `OakERP.API/Program.cs`
- `OakERP.Tests.Unit/Posting/ArInvoicePostingEngineTests.cs`
- `OakERP.Tests.Unit/Posting/PostingServiceTests.cs`
- `OakERP.Tests.Unit/Posting/PostingServiceTestFactory.cs`
- `OakERP.Tests.Integration/Posting/ArInvoicePostingTests.cs`
- `OakERP.Tests.Integration/TestSetup/OakErpWebFactory.cs`
- `OakERP.Tests.Integration/TestSetup/WebApiIntegrationTestBase.cs`

## Validation
- Final validation completed in the approved order:
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj`
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ArInvoicePosting`
- `dotnet build OakERP.sln`

## Remaining
- None for Slice 1A inside the approved scope.

## Outcome
- Completed.

## Next Recommended Step
- Plan Slice 1B as the inventory extension only: line location, inventory movements, COGS/inventory asset GL, and moving-average costing.
