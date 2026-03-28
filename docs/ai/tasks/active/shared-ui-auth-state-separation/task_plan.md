## Scope

Implement only the narrow auth UI state separation slice by moving auth form models, auth view models, and their registration out of `OakERP.Shared` into `OakERP.UI`.

## Constraints

- Do not move Razor pages.
- Do not move shared layouts, routes, navigation helpers, or host abstractions.
- Do not widen `OakERP.UI` into a general feature-module project.
- Do not force page moves, route changes, or host composition redesign.
- Avoid renames unless strictly necessary.
- Preserve existing auth page behavior exactly.

## Ordered Steps

1. Create a minimal `OakERP.UI` class library that can host auth UI state without referencing `OakERP.Shared`.
2. Move auth form models and auth view models into `OakERP.UI`.
3. Move auth UI-state registration into `OakERP.UI` and let `OakERP.Shared` delegate to it.
4. Update `OakERP.Shared` to reference `OakERP.UI`.
5. Add `OakERP.UI` to the solution.
6. Run targeted builds first, then broader tests/build.

## Validation Plan

1. `dotnet build OakERP.UI/OakERP.UI.csproj`
2. `dotnet build OakERP.Shared/OakERP.Shared.csproj`
3. `dotnet build OakERP.Web/OakERP.Web.csproj`
4. `dotnet build OakERP.Desktop/OakERP.Desktop.csproj`
5. `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --no-restore`
6. `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --no-restore`
7. `dotnet build OakERP.sln`
