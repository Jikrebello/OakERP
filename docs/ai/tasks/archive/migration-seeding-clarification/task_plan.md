# Migration Seeding Clarification

## Scope

Implement only the approved minimal migration/seeding clarification slice:
- identify and reduce overlapping migration/seeding entry points
- make the intended migration path per environment explicit
- make seeding ownership clearer
- preserve current runtime behavior

## Constraints

- do not redesign auth/domain/application boundaries
- do not split shared UI/client projects
- do not change public API behavior
- do not change when seeding runs in Development or Testing
- do not change default environment values unless necessary
- keep the change set minimal
- prefer script comment/help-text clarification over script behavior changes
- stop if hidden `DbInitializer` usage or workflow ambiguity appears

## Ordered Steps

1. Confirm whether `DbInitializer` has any remaining references.
2. Record the current migration/seeding entry points and ownership in `findings.md`.
3. Remove only the redundant, unused `DbInitializer` path if confirmed unused.
4. Clarify migration ownership in script comments/help text without changing script behavior.
5. Record what changed and what remained intentionally unchanged in `progress.md`.
6. Run relevant builds/tests and note whether failures are regressions or environment issues.

## Validation Plan

1. `dotnet build OakERP.API/OakERP.API.csproj`
2. `dotnet build OakERP.MigrationTool/OakERP.MigrationTool.csproj`
3. `dotnet build OakERP.sln`
4. `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --no-restore`
5. `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --no-restore`
