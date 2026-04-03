## Progress

- Created task folder for the shared client plumbing separation slice.
- Confirmed `OakERP.Shared` still mixes UI with clearly non-UI client plumbing.
- Confirmed `OakERP.Mobile` is not aligned with the shared client stack and will stay out of scope.
- Confirmed `AuthRoutes` had to move with client plumbing to avoid a `Client -> Shared` project-reference cycle.
- Added a minimal `OakERP.Client` project and moved only non-UI client plumbing into it:
  - API client/result/handler
  - auth/session services
  - client service interfaces
  - `AuthRoutes`
- Kept current namespaces for moved client types to avoid broader rename churn.
- Updated `OakERP.Shared` to reference `OakERP.Client` and keep only UI-facing registration plus auth view models.
- Added `OakERP.Client` to the solution.
- Sequential validation passed:
  - `dotnet build OakERP.Client/OakERP.Client.csproj`
  - `dotnet build OakERP.Shared/OakERP.Shared.csproj`
  - `dotnet build OakERP.Web/OakERP.Web.csproj`
  - `dotnet build OakERP.Desktop/OakERP.Desktop.csproj`
  - `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --no-restore`
  - `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --no-restore`
  - `dotnet build OakERP.sln`

## Validation Notes

- No environment or file-lock noise occurred during the sequential validation run.
- `dotnet test` for integration surfaced the existing nullable warning in `OakERP.API/Controllers/BaseController.cs`, but this slice did not modify API code or change test behavior.

## Pending

- No remaining work in this slice.
