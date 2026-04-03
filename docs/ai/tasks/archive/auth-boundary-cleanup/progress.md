## Progress

- Created task folder for the auth boundary cleanup slice.
- Confirmed the live `Auth -> Infrastructure` dependency is limited to Identity registration ownership plus the matching project reference.
- Confirmed the proposed move does not require changing `ApplicationUser`, EF schema, or migrations.
- Moved `AddIdentityServices()` ownership from `OakERP.Auth` into `OakERP.Infrastructure`, preserving the existing Identity options and store registration.
- Removed the direct `OakERP.Auth -> OakERP.Infrastructure` project reference.
- Added the direct `OakERP.Auth -> OakERP.Application` project reference that was already required by `IUnitOfWork` usage in `AuthService`.
- Added `Microsoft.AspNetCore.App` as a framework reference in `OakERP.Infrastructure` so the relocated Identity DI registration compiles in its new home.
- Sequential validation passed:
  - `dotnet build OakERP.Auth/OakERP.Auth.csproj`
  - `dotnet build OakERP.API/OakERP.API.csproj`
  - `dotnet build OakERP.MigrationTool/OakERP.MigrationTool.csproj`
  - `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --no-restore`
  - `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --no-restore`
  - `dotnet build OakERP.sln`

## Validation Notes

- Initial `OakERP.Auth` build failed after removing the Infrastructure reference because `IUnitOfWork` had been arriving transitively; this was a real dependency issue and was fixed by adding a direct `OakERP.Application` reference.
- Initial `OakERP.API` build failed because `OakERP.Infrastructure` needed the ASP.NET Core shared framework to compile the relocated `AddIdentity(...)` registration; this was a real compile issue and was fixed by adding the framework reference.

## Pending

- No remaining work in this slice.
