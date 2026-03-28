# Findings

## Current Entry Points

- `OakERP.API/Program.cs`
  - registers seeders
  - registers `SeedCoordinator`
  - registers `DbInitializer`
  - actually uses only `SeedCoordinator`
  - seeds on startup when `RunSeedOnStartup=true`
  - does not run `Database.MigrateAsync()`

- `OakERP.MigrationTool/Main.cs`
  - registers seeders
  - registers `SeedCoordinator`
  - runs `Database.MigrateAsync()`
  - then runs `SeedCoordinator.RunAsync(environment)`

- `OakERP.Infrastructure/Persistence/DbInitializer.cs`
  - duplicates seeding orchestration already handled by `SeedCoordinator`
  - confirmed unused except for API DI registration

- `migrate.ps1`
  - documents multiple ways to combine EF update and MigrationTool
  - operational behavior currently allows both EF CLI and MigrationTool to participate

## Confirmed Reference Check

`DbInitializer` search results show only:
- `OakERP.API/Program.cs`
- `OakERP.Infrastructure/Persistence/DbInitializer.cs`

No other references were found in the repository.

## Minimal Safe Clarification

- Keep `MigrationTool` as the explicit schema migration path.
- Keep API startup seeding behavior unchanged.
- Keep `SeedCoordinator` as the single active seeding coordinator.
- Remove `DbInitializer` only because it is redundant and unused.
- Clarify script comments/help text instead of changing script behavior.
