# Progress

## Completed

- Created task folder for the wave 12-14 maintainability slice.
- Confirmed the remaining duplication is concentrated in AP/AR settlement-document workflows, duplicate MAUI host adapters, and a few transaction/persistence orchestration seams.
- Replaced AP/AR-specific settlement workflow dependency bundles with one shared `SettlementDocumentWorkflowDependencies` bundle.
- Added shared settlement-document precondition helpers plus create/allocation workflow runners and moved AP payment and AR receipt workflows onto them.
- Added shared application orchestration helpers for result-based transaction execution and exception translation, then applied them to settlement workflows and `ApInvoiceCreateWorkflow`.
- Added `AuthTransactionRunner` and moved auth registration transaction handling onto it so failure results roll back consistently without local rollback calls.
- Added shared MAUI host-core source under `OakERP.Shared/Hosts/Maui`, linked it into Desktop and Mobile, and reduced both `MauiProgram.cs` files to shared host composition plus local UI setup.
- Removed the duplicate Desktop and Mobile MAUI adapter classes that were previously copy-equivalent.
- Added direct unit coverage for the new transaction and workflow helper seams.

## Validation

- `dotnet build OakERP.Application/OakERP.Application.csproj /nr:false /m:1`
- `dotnet build OakERP.Auth/OakERP.Auth.csproj /nr:false /m:1`
- `dotnet build OakERP.Tests.Unit/OakERP.Tests.Unit.csproj /nr:false /m:1`
- `dotnet build OakERP.Shared/OakERP.Shared.csproj /nr:false /m:1`
- `dotnet build OakERP.Web/OakERP.Web.csproj /nr:false /m:1`
- `dotnet build OakERP.Desktop/OakERP.Desktop.csproj /nr:false /m:1`
- `dotnet build OakERP.Mobile/OakERP.Mobile.csproj /nr:false /m:1`
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj /nr:false /m:1`
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj /nr:false /m:1`
- `dotnet build OakERP.sln /nr:false /m:1`

## Validation Notes

- An initial parallel host-build attempt caused transient file-lock failures in `OakERP.Common`, `OakERP.UI`, and static-web-assets cache files. Sequential reruns passed cleanly and the issue was environmental, not code-related.
- Unit tests passed: 118.
- Integration tests passed: 73.

## Pending

- No remaining work in this slice.

## Deferred Risks

- The repo already contains unrelated uncommitted changes outside this slice; they were left untouched.
