# Progress

## Task
refactor-waves-8-11

## Started
2026-04-08 19:49:18

## Work Log
- Replaced application/auth result error semantics with `FailureKind`-based failures and moved HTTP status resolution fully into API transport code.
- Removed `HttpStatusCode` from the common exception base and updated API exception handling to translate semantic exceptions through API-side status resolution.
- Split `AuthService` into a thin coordinator over internal registration/login workflows plus audit and identity-failure helpers, and moved Auth/JWT time access to `IClock`.
- Split `ApPaymentService`, `ArReceiptService`, and `ApInvoiceService` into thin service entrypoints over focused internal workflows.
- Added `OakERP.Shared.Extensions.ServiceCollectionExtensions.AddOakSharedHostServices(...)` and aligned Web/Desktop/Mobile to the same client/auth/UI bootstrap path.
- Added Mobile host services and shared routes wiring so Mobile now composes the same shared client stack as Web/Desktop.
- Split the oversized posting/runtime test files into smaller concern-based files and updated runtime/unit assertions to the new failure model.

## Files Touched
- `OakERP.Common/Errors`
- `OakERP.Common/Exceptions`
- `OakERP.Common/Time`
- `OakERP.Application/AccountsPayable`
- `OakERP.Application/AccountsReceivable`
- `OakERP.Application/Settlements`
- `OakERP.Auth/Services`
- `OakERP.Auth/Jwt`
- `OakERP.Auth/Extensions`
- `OakERP.API/Errors`
- `OakERP.API/Extensions`
- `OakERP.Shared/Extensions`
- `OakERP.Web/Program.cs`
- `OakERP.Desktop/MauiProgram.cs`
- `OakERP.Mobile/*`
- `OakERP.Tests.Unit/*`
- `OakERP.Tests.Integration/Runtime/*`

## Validation
- `dotnet build OakERP.Auth/OakERP.Auth.csproj /nr:false /m:1`
- `dotnet build OakERP.Application/OakERP.Application.csproj /nr:false /m:1`
- `dotnet build OakERP.Shared/OakERP.Shared.csproj /nr:false /m:1`
- `dotnet build OakERP.Web/OakERP.Web.csproj /nr:false /m:1`
- `dotnet build OakERP.Desktop/OakERP.Desktop.csproj /nr:false /m:1`
- `dotnet build OakERP.Mobile/OakERP.Mobile.csproj /nr:false /m:1`
- `dotnet build OakERP.Tests.Unit/OakERP.Tests.Unit.csproj /nr:false /m:1`
- `dotnet build OakERP.Tests.Integration/OakERP.Tests.Integration.csproj /nr:false /m:1`
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj /nr:false /m:1`
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj /nr:false /m:1`
- `dotnet build OakERP.sln /nr:false /m:1`

## Remaining
- No blocking implementation work remains for the planned waves.

## Deferred Smells / Risks
- AP/AR family workflows still mirror each other in places, even though the service entrypoints are now thinner.
- Host adapter implementations remain host-local and intentionally separate.

## Outcome
- All four planned waves were implemented without changing routes, DTO wire shape, schema, or auth token format.

## Next Recommended Step
- If another cleanup wave is needed, target the remaining AP/AR workflow parallelism or the host-local adapter duplication, not another broad structural rewrite.
