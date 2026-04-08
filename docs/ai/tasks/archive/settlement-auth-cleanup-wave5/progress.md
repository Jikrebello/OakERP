# Progress

## Started

- Opened wave-5 task records for settlement/auth cleanup.
- Confirmed the settlement warnings come from the generic/spec shape, not from AP/AR service behavior.
- Confirmed `ApInvoiceService` and `AuthService` are the two main readability hotspots in the current warning batch.

## Completed

- Replaced the settlement invoice-load and allocation-apply spec shapes with smaller grouped models.
- Normalized allocation inputs into a shared `SettlementAllocationInput` value so the applicator no longer carries document/invoice/input generic parameters.
- Updated AP payment and AR receipt settlement adapters and services to bind document/invoice behavior through closures instead of large generic delegate bags.
- Updated the dedicated settlement unit tests to the new settlement shape.
- Split `ApInvoiceService.CreateAsync` into smaller private validation/build helpers to reduce cognitive complexity.
- Refactored `AuthService` registration and login flows into smaller private workflow helpers while preserving audit behavior and messages.

## Validation

- `dotnet build OakERP.Application/OakERP.Application.csproj /nr:false /m:1`
- `dotnet build OakERP.Auth/OakERP.Auth.csproj /nr:false /m:1`
- `dotnet build OakERP.Tests.Unit/OakERP.Tests.Unit.csproj /nr:false /m:1`
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj /nr:false /m:1`
  - Passed: 101
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj /nr:false /m:1`
  - Passed: 72
- `dotnet build OakERP.sln /nr:false /m:1`
  - Passed with 0 warnings and 0 errors

## Deferred Risks

- None recorded yet for this wave.
