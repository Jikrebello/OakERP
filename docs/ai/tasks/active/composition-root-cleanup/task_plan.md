# Composition Root Cleanup

## Scope

Implement only Phase 1 / the first implementation slice:
- move shared DI/bootstrap ownership out of `OakERP.API`
- remove `OakERP.MigrationTool -> OakERP.API`
- preserve runtime behavior

## Constraints

- keep the change set as small as possible
- do not introduce new projects or assemblies
- do not change controllers, endpoints, DTOs, CORS behavior, client base URLs, seeding behavior, or domain model design
- do not redesign auth/domain/application boundaries
- stop and report if the extraction introduces circular project references or requires larger architectural redesign

## Ordered Steps

1. Record current findings and baseline validation state.
2. Extract persistence/repository/seeder registration into `OakERP.Infrastructure`.
3. Extract Identity/JWT/auth registration into `OakERP.Auth`.
4. Update `OakERP.API` to consume the new layer-owned extensions while leaving API-only registration in API.
5. Update `OakERP.MigrationTool` to consume the new layer-owned extensions and remove the API project reference.
6. Run targeted builds, solution build, unit tests, and integration tests.
7. Record outcomes, remaining risks, and next phase in `progress.md`.

## Validation Plan

1. `dotnet build OakERP.API/OakERP.API.csproj`
2. `dotnet build OakERP.MigrationTool/OakERP.MigrationTool.csproj`
3. `dotnet build OakERP.sln`
4. `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj`
5. `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj`

Integration test failures caused by unavailable PostgreSQL on `localhost:5433` should be recorded as pre-existing environment/test-infrastructure failures unless the refactor introduces a different failure mode.
