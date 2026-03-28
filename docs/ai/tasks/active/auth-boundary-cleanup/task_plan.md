## Scope

Implement only the narrow auth/application boundary cleanup slice that reduces the concrete `OakERP.Auth -> OakERP.Infrastructure` dependency without changing the identity model, schema, migrations, public API shape, or auth behavior.

## Constraints

- Preserve login/register/tenant/license behavior exactly.
- Do not redesign or move `ApplicationUser`.
- Do not touch EF/Identity schema or migrations.
- Do not add new projects unless absolutely necessary.
- Skip cosmetic contract moves that do not remove a concrete dependency problem.

## Ordered Steps

1. Confirm the live `Auth -> Infrastructure` dependency path in source.
2. Move only Identity store registration ownership out of `OakERP.Auth` into a neutral existing layer.
3. Update API and MigrationTool to consume the relocated registration.
4. Remove the now-unused `OakERP.Auth -> OakERP.Infrastructure` project reference if no other source references remain.
5. Validate targeted builds/tests first, then broader solution validation.

## Validation Plan

1. `dotnet build OakERP.Auth/OakERP.Auth.csproj`
2. `dotnet build OakERP.API/OakERP.API.csproj`
3. `dotnet build OakERP.MigrationTool/OakERP.MigrationTool.csproj`
4. `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --no-restore`
5. `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --no-restore`
6. `dotnet build OakERP.sln`
