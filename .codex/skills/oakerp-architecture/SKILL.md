---
name: oakerp-architecture
description: Architecture audits and refactors for OakERP focused on dependency direction, composition-root placement, configuration hygiene, migration and seeding clarity, and shared client or UI boundaries. Use when a task changes multiple projects, service registration, or structural ownership.
---

# OakERP Architecture Skill

Use this skill for architecture, maintainability, and code-organization work in OakERP.
Keep the scope structural: dependency direction, composition roots, configuration hygiene, migration and seeding clarity, and test boundaries.
Do not use it for broad UI polish or speculative rewrites.

## Read First

Inspect at least:
- `AGENTS.md`
- `docs/architecture/dependency-rules.md`
- relevant `.csproj` files for affected projects
- the nearest existing task files under `docs/ai/tasks/active/` if the task already has a folder

## Core Rules

1. Preserve behavior unless the task explicitly allows behavior changes.
2. Prefer the smallest coherent architectural improvement.
3. Fix dependency direction before doing cosmetic cleanup.
4. Do not combine unrelated refactors in a single change.
5. Validate builds or tests after edits.
6. Apply SOLID and DRY pragmatically: solve real coupling and duplication, not hypothetical future problems.

## Default Workflow

### 1. Audit First

Before editing:
- inspect project references
- identify the current dependency path
- identify the desired dependency path
- list the minimum set of files or projects required to improve the situation
- check recent migrations for `Up()` and `Down()` symmetry if the task touches schema work
- check whether similarly named runtime and persisted models are being mixed together
- check whether business-significant literals are scattered as raw strings
- check whether thin services or pure engines and calculators are starting to absorb responsibilities they were meant to avoid

If the task is larger than a tiny local edit, create:

`docs/ai/tasks/active/<task-slug>/`

with:
- `task_plan.md`
- `findings.md`
- `progress.md`

### 2. Plan The Smallest Safe Slice

Prefer one of these slice types:
- composition root cleanup
- configuration externalization
- migration and seeding consolidation
- test harness cleanup
- shared client and UI separation

Do not try to solve all five in one step.

### 3. Edit Conservatively

Preferred refactor patterns:
- extract shared DI and bootstrapping from API into neutral extension locations
- replace hardcoded runtime values with config and options
- consolidate overlapping startup or seed paths
- move contracts upward and implementations downward
- split shared code by responsibility, not by arbitrary folder count

Avoid:
- churny renames with no structural gain
- inventing new abstractions without immediate use
- moving files just to satisfy aesthetics
- mixing deployment changes into architecture refactors unless explicitly requested
- extracting helper layers unless they remove a concrete ownership, duplication, or dependency problem

### 4. Validate

From repo root, use the smallest relevant command first.

Examples:

```powershell
dotnet build OakERP.sln
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj
```

If a narrower project build or test is sufficient, start there.

If validation fails:
- record the exact failure in `progress.md`
- explain whether it is caused by your change or pre-existing state

### 5. Add Tests When Behavior Moves

If a change introduces or changes behavior, add or update tests in the same slice unless explicitly told not to.

Preferred defaults:
- unit tests for pure logic, mapping, orchestration seams, and service behavior
- integration tests for API, runtime, persistence, and transaction behavior
- if the task touches transactional writes, include or preserve proof that failure paths leave no partial writes

Do not leave behavior changes untested just because the solution builds.

## Review Heuristics

A change is good if it does most of these:
- lowers coupling
- improves dependency direction
- makes configuration more external
- makes runtime behavior easier to reason about
- removes duplicated ownership or responsibility
- keeps code discoverable

A change is suspect if it does most of these:
- adds another indirection layer without payoff
- spreads service wiring across more places
- moves logic into shared code for convenience
- preserves the same coupling but under a new name
- increases file count without reducing complexity

Audit specifically for:
- migrations whose rollback path drops unrelated objects
- runtime model namespaces that import persisted entity types
- tests that appear to prove fallback order but only use pre-resolved values
- domain-significant magic strings or numbers that should be reviewed
- orchestration classes that are deciding too much
- engines or calculators that stopped being pure

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

with config and options.

### Migration And Seeding

Aim for one clear migration path and one clear seeding path per environment.
Avoid overlapping startup-time and script-time ownership.

### Shared Client And UI

Do not let shared code become a junk drawer.
Separate:
- UI building blocks
- client infrastructure
- auth and session plumbing
- feature-specific screens or view models

when a task explicitly targets this boundary.

## Tool Preference

If Serena is active and available, prefer it for symbol-aware discovery before editing:
- find symbol
- find references
- find implementations
- trace call paths
- identify minimal touched files

Do not require repeated user prompting to use Serena when the task clearly benefits from it.

## Output Expectations

When you finish a refactor, summarize:
1. what changed
2. why it was changed
3. what dependency, config, or ownership problem it fixed
4. what validation ran
5. any follow-up work still recommended
