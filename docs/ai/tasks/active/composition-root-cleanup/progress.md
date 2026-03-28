# Progress

## Started

- Scope locked to Phase 1 composition-root cleanup only.
- Confirmed the neutral extraction path does not require new projects or broader boundary redesign.

## In Progress

- Creating task tracking files.
- Extracting shared registrations into `OakERP.Infrastructure` and `OakERP.Auth`.
- Updating `OakERP.API` and `OakERP.MigrationTool` to consume layer-owned extensions.

## Completed

- Added `OakERP.Infrastructure.Extensions.ServiceCollectionExtensions` for persistence, repository, and seeder registration.
- Added `OakERP.Auth.Extensions.ServiceCollectionExtensions` for Identity, JWT, and auth service registration.
- Reduced `OakERP.API` service-collection extensions to API-owned Swagger registration only.
- Updated `OakERP.API` and `OakERP.MigrationTool` to consume the new layer-owned extensions.
- Removed the direct `OakERP.MigrationTool -> OakERP.API` project reference.

## Validation

- `dotnet build OakERP.API/OakERP.API.csproj`: passed
- `dotnet build OakERP.MigrationTool/OakERP.MigrationTool.csproj`: passed
- `dotnet build OakERP.sln`: passed
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --no-restore`: passed
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --no-restore`: passed

## Validation Notes

- Initial `dotnet build OakERP.API/OakERP.API.csproj` failed because `OakERP.Infrastructure.dll` was temporarily locked by Microsoft Defender. This is an external file-lock issue, not a refactor regression.
- Initial `dotnet build OakERP.MigrationTool/OakERP.MigrationTool.csproj` exposed one real regression: after moving JWT registration into `OakERP.Auth`, `OakERP.Auth` needed the `Microsoft.AspNetCore.Authentication.JwtBearer` package reference that had previously lived only in API.
- After adding that package reference and rerunning sequentially, all requested builds and tests passed.

## Deferred Intentionally

- configuration externalization
- migration/seeding flow consolidation
- test architecture cleanup
- auth/domain/application boundary redesign
- shared client/UI split
