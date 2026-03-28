## Scope

Implement only the smallest shared client/UI boundary cleanup slice by moving clearly non-UI client plumbing out of `OakERP.Shared` while preserving current routes, layouts, rendering behavior, auth flow behavior, and API contracts.

## Constraints

- Do not create a big new project/module layout.
- Prefer moving only clearly non-UI client plumbing out of `OakERP.Shared`.
- Do not move view models unless directly required.
- Do not try to fully align `OakERP.Mobile` in this slice.
- Do not change routes, layouts, rendering behavior, auth flow behavior, or API contracts.
- If the separation would require a broader feature-boundary or host-composition redesign, stop and report the dependency conflict.
- Avoid renames unless strictly necessary.

## Ordered Steps

1. Confirm whether there is an existing safe home for non-UI client plumbing.
2. Create the smallest viable client-plumbing project only if no suitable existing project already exists.
3. Move only non-UI client plumbing types and registrations out of `OakERP.Shared`.
4. Keep `OakERP.Shared` focused on shared UI and view-model registration.
5. Update host projects only where required to keep current behavior intact.
6. Validate targeted builds first, then broader solution/tests.

## Validation Plan

1. `dotnet build OakERP.Client/OakERP.Client.csproj`
2. `dotnet build OakERP.Shared/OakERP.Shared.csproj`
3. `dotnet build OakERP.Web/OakERP.Web.csproj`
4. `dotnet build OakERP.Desktop/OakERP.Desktop.csproj`
5. `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --no-restore`
6. `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --no-restore`
7. `dotnet build OakERP.sln`
