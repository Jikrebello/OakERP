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
- Externalized API CORS origins into configuration.
- Externalized Web and Desktop API base URL lookup into configuration-driven values while preserving the existing default URL.
- Moved seed defaults into explicit API configuration and removed code literals from the seeder.
- Centralized integration-test DB connection lookup with config and environment-variable overrides.

## Validation

- `dotnet build OakERP.API/OakERP.API.csproj`: passed
- `dotnet build OakERP.MigrationTool/OakERP.MigrationTool.csproj`: passed
- `dotnet build OakERP.sln`: passed
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --no-restore`: passed
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --no-restore`: passed
- Phase 2 rerun: `dotnet build OakERP.sln`: passed
- Phase 2 rerun: `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --no-restore`: passed
- Phase 2 rerun: `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --no-restore`: passed

## Validation Notes

- Initial `dotnet build OakERP.API/OakERP.API.csproj` failed because `OakERP.Infrastructure.dll` was temporarily locked by Microsoft Defender. This is an external file-lock issue, not a refactor regression.
- Initial `dotnet build OakERP.MigrationTool/OakERP.MigrationTool.csproj` exposed one real regression: after moving JWT registration into `OakERP.Auth`, `OakERP.Auth` needed the `Microsoft.AspNetCore.Authentication.JwtBearer` package reference that had previously lived only in API.
- After adding that package reference and rerunning sequentially, all requested builds and tests passed.
- During Phase 2 validation, the first parallel `dotnet test` run hit build-artifact file locks in `obj\Debug` because multiple `dotnet` processes were running concurrently. This was a validation-environment issue, not a code regression.
- Re-running the two test commands sequentially produced clean passes for both unit and integration tests.

## Deferred Intentionally

- migration/seeding flow consolidation
- test architecture cleanup
- auth/domain/application boundary redesign
- shared client/UI split
