# Composition Root Cleanup And Config Cleanup

## Scope

Implement the approved slices only:
- Phase 1 composition-root cleanup
- Phase 2 configuration externalization and options cleanup

Current Phase 2 scope:
- replace hardcoded CORS origins with configuration-driven values
- replace hardcoded API base URLs in clients with configuration-driven values
- move seed defaults into explicit configuration where appropriate
- reduce hardcoded integration-test DB assumptions where possible without redesigning the test architecture
- preserve runtime behavior

## Constraints

- keep the change set as small as possible
- do not introduce new projects or assemblies
- do not change controllers, endpoints, DTOs, CORS behavior, client base URLs, seeding behavior, or domain model design
- do not redesign auth/domain/application boundaries
- stop and report if the extraction introduces circular project references or requires larger architectural redesign

## Ordered Steps

1. Record current findings and baseline validation state.
2. Externalize CORS origin values into API configuration.
3. Externalize client API base URLs into host configuration or host configuration lookup.
4. Move seed defaults into explicit configuration and remove code literals where safe.
5. Reduce repeated integration-test DB connection literals by centralizing config/env lookup.
6. Run targeted builds, solution build, unit tests, and integration tests.
7. Record outcomes, remaining risks, and next phase in `progress.md`.

## Validation Plan

1. `dotnet build OakERP.API/OakERP.API.csproj`
2. `dotnet build OakERP.MigrationTool/OakERP.MigrationTool.csproj`
3. `dotnet build OakERP.sln`
4. `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj`
5. `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj`

Integration test failures caused by unavailable PostgreSQL on `localhost:5433` should be recorded as pre-existing environment/test-infrastructure failures unless the refactor introduces a different failure mode.
