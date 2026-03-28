# Findings

## Pre-change Facts

- `OakERP.API/Extensions/ServiceCollectionExtensions.cs` owned shared registration for:
  - `AddApplicationDb`
  - `AddSeedersFromAssemblies`
  - `AddIdentityServices`
  - `AddJwtAuth`
  - `AddPersistenceServices`
  - `AddAuthServices`
  - `AddRepositories`
- `OakERP.API/Program.cs` consumed those methods directly.
- `OakERP.MigrationTool/Main.cs` imported `OakERP.API.Extensions` to reuse part of that shared registration.
- `OakERP.MigrationTool/OakERP.MigrationTool.csproj` had a direct `ProjectReference` to `OakERP.API`.
- `OakERP.MigrationTool` also linked API appsettings files into its output directory. This is intentionally preserved in Phase 1 to avoid configuration behavior changes.

## Ownership Direction For This Slice

- `OakERP.Infrastructure` is the correct existing home for:
  - DbContext registration
  - seeder scanning registration
  - unit of work registration
  - repository registration
- `OakERP.Auth` is the correct existing home for:
  - Identity registration
  - JWT authentication registration
  - auth service registration
- `OakERP.API` should retain API-only concerns such as Swagger registration and middleware wiring.

## Dependency Check

- Moving persistence/repository registration into `OakERP.Infrastructure` does not create a new reverse dependency.
- Moving auth registration into `OakERP.Auth` does not create a new reverse dependency because `OakERP.Auth` already references `OakERP.Infrastructure`.
- No `Infrastructure -> Auth -> Infrastructure` loop is introduced by this Phase 1 extraction.
