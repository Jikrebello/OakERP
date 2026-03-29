# OakERP Architecture Skill

## Purpose

Use this skill when auditing or refactoring OakERP for architecture, maintainability, or code-organization work.

This skill is about structure, dependency direction, composition roots, configuration hygiene, migration/seeding clarity, and test boundaries.

It is **not** for broad UI polish or speculative rewrites.

## Required Inputs

Before making changes, inspect at least:

- `AGENTS.md`
- `docs/architecture/dependency-rules.md`
- relevant `.csproj` files for affected projects
- the nearest existing task files under `docs/ai/tasks/active/` if the task already has a folder

## Core Rules

1. Preserve behavior unless the task explicitly allows behavior changes.
2. Prefer the smallest coherent architectural improvement.
3. Fix dependency direction before doing cosmetic cleanup.
4. Do not combine unrelated refactors in a single change.
5. Validate builds/tests after edits.

## Default Workflow

### 1. Audit First

Before editing:

- inspect project references
- identify current dependency path
- identify the desired dependency path
- list the minimum set of files/projects required to improve the situation

If the task is larger than a tiny local edit, create:

`docs/ai/tasks/active/<task-slug>/`

with:
- `task_plan.md`
- `findings.md`
- `progress.md`

### 2. Plan the Smallest Safe Slice

Prefer one of these slice types:

- composition root cleanup
- configuration externalization
- migration/seeding consolidation
- test harness cleanup
- shared client/UI separation

Do **not** try to solve all five in one step.

### 3. Edit Conservatively

Preferred refactor patterns:

- extract shared DI/bootstrapping from API into neutral extension locations
- replace hardcoded runtime values with config/options
- consolidate overlapping startup/seed paths
- move contracts upward and implementations downward
- split shared code by responsibility, not by arbitrary folder count

Avoid:

- churny renames with no structural gain
- inventing new abstractions without immediate use
- moving files just to satisfy aesthetics
- mixing deployment changes into architecture refactors unless explicitly requested

### 4. Validate

From repo root, use the smallest relevant command first.

Examples:

```powershell
dotnet build OakERP.sln
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj
```

If a narrower project build/test is sufficient, start there.

If validation fails:
- record the exact failure in `progress.md`
- explain whether it is caused by your change or pre-existing state

### 5. Add Tests When Behavior Moves

If a change introduces or changes behavior, add or update tests in the same slice unless explicitly told not to.

Preferred defaults:
- unit tests for pure logic, mapping, orchestration seams, and service behavior
- integration tests for API/runtime/persistence/transaction behavior

Do not leave behavior changes untested just because the solution builds.

## Review Heuristics

A change is good if it does most of these:

- lowers coupling
- improves dependency direction
- makes configuration more external
- makes runtime behavior easier to reason about
- removes duplicated ownership/responsibility
- keeps code discoverable

A change is suspect if it does most of these:

- adds another indirection layer without payoff
- spreads service wiring across more places
- moves logic into shared code "for convenience"
- preserves the same coupling but under a new name
- increases file count without reducing complexity

## Project-Specific Guidance

### Composition Root

If both API and MigrationTool need the same registration, that wiring does not belong exclusively to API.

Prefer neutral extension locations in the correct layer.

### Domain

Do not introduce new framework-heavy dependencies into Domain.

If you encounter existing framework coupling in Domain, document it as technical debt and avoid extending it.

### Configuration

Replace hardcoded:
- localhost URLs
- CORS origins
- connection strings
- machine-specific test assumptions

with config + options.

### Migration and Seeding

Aim for one clear migration path and one clear seeding path per environment.

Avoid overlapping startup-time and script-time ownership.

### Shared Client/UI

Do not let shared code become a junk drawer.

Separate:
- UI building blocks
- client infrastructure
- auth/session plumbing
- feature-specific screens/view models

when a task explicitly targets this boundary.

## Output Expectations

When you finish a refactor, summarize:

1. what changed
2. why it was changed
3. what dependency/config/ownership problem it fixed
4. what validation ran
5. any follow-up work still recommended
