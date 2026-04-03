## Progress

- Created task folder for the auth UI state separation slice.
- Confirmed the live auth UI state in `OakERP.Shared` is limited to:
  - `LoginFormModel`
  - `RegisterFormModel`
  - `LoginViewModel`
  - `RegisterViewModel`
  - their DI registration
- Confirmed that reusing `BaseFormViewModel` from `OakERP.Shared` would create a project cycle.
- Added a minimal `OakERP.UI` project.
- Moved only the auth form models and auth view models into `OakERP.UI`.
- Moved auth UI-state registration into `OakERP.UI` and updated `OakERP.Shared` to delegate to it.
- Preserved the existing `OakERP.Shared.Models.Auth` and `OakERP.Shared.ViewModels.Auth` namespaces to avoid page churn.
- Left Razor pages, shared layouts, routes, navigation helpers, and host abstractions untouched.
- Added `OakERP.UI` to the solution.
- Sequential validation passed:
  - `dotnet build OakERP.UI/OakERP.UI.csproj`
  - `dotnet build OakERP.Shared/OakERP.Shared.csproj`
  - `dotnet build OakERP.Web/OakERP.Web.csproj`
  - `dotnet build OakERP.Desktop/OakERP.Desktop.csproj`
  - `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --no-restore`
  - `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --no-restore`
  - `dotnet build OakERP.sln`

## Validation Notes

- Initial `OakERP.Desktop` build failed because `OakERP.UI` used a `Microsoft.AspNetCore.App` framework reference, which does not resolve a Mac Catalyst runtime pack through the MAUI target chain. This was a real cross-targeting issue introduced by the slice and was fixed by switching `OakERP.UI` to the `Microsoft.AspNetCore.Components.Web` package pattern already used in `OakERP.Shared`.
- Integration tests surfaced the existing nullable warning in `OakERP.API/Controllers/BaseController.cs`, but this slice did not modify API behavior or test setup.

## Pending

- No remaining work in this slice.
