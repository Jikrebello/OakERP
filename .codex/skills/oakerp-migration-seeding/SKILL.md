---
name: oakerp-migration-seeding
description: Migration and seeding work for OakERP. Use when changing EF Core migrations, rollback behavior, seeding ownership, MigrationTool wiring, DbContext setup, SQL view seeding, or migration scripts and environment flow.
---

# OakERP Migration Seeding

Use this skill when a task touches schema changes, migration ownership, seeding flow, or the operational tooling around them.
Optimize for one clear migration story per environment and keep rollback behavior exact.

## Read First

Inspect:
- `AGENTS.md`
- `docs/architecture/dependency-rules.md`
- `docs/architecture/project-map.md`
- `docs/ai/tasks/archive/migration-seeding-clarification/progress.md`
- `docs/ai/tasks/archive/composition-root-cleanup/progress.md`

Trace the active path through:
- `OakERP.Infrastructure/Persistence`
- `OakERP.Infrastructure/Migrations`
- `OakERP.Infrastructure/Extensions/ServiceCollectionExtensions.cs`
- `OakERP.MigrationTool`
- `OakERP.API/Program.cs`
- `migrate.ps1`

## Core Rules

- Keep `MigrationTool` independent from API-specific service registration.
- Keep each migration `Down()` limited to reversing that migration's `Up()`.
- Avoid overlapping ownership between startup-time seeding, tool-driven seeding, and ad hoc scripts.
- Keep connection strings, CORS, and environment-specific values externalized.
- Prefer the smallest change that clarifies ownership instead of redesigning the full flow.
- Record rollback review explicitly when a migration is involved.

## Workflow

### 1. Identify The Actual Owner

Confirm which executable or layer owns:
- service registration
- schema application
- seed coordination
- SQL view creation
- environment-specific entry points

If two executables need the same registration, move it to a neutral layer-owned extension instead of making one depend on the other.

### 2. Keep The Change Narrow

Prefer focused slices such as:
- removing a dead seeding path
- relocating shared registration out of API
- clarifying script ownership or comments
- fixing migration rollback
- consolidating one ambiguous seeding decision point

Avoid combining migration flow cleanup with unrelated feature work.

### 3. Validate The Operational Path

Use the smallest relevant commands first, for example:

```powershell
dotnet build OakERP.MigrationTool/OakERP.MigrationTool.csproj
dotnet build OakERP.API/OakERP.API.csproj
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --no-restore
```

If the task changes a migration, also inspect the generated rollback script or equivalent proof instead of assuming `Down()` is correct.

## Review Checklist

- Is there one clear owner for migration and seeding behavior?
- Did `MigrationTool` stay free of API-only wiring?
- Did rollback validation examine the actual reversed operations?
- Were no new hardcoded environment values introduced?
- Did task docs capture any deferred operational ambiguity?
