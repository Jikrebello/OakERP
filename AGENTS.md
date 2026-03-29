# AGENTS.md

## Purpose

OakERP is an early-stage ERP system being shaped toward clean architecture, enterprise maintainability, and operational clarity.

This repository already has meaningful separation between API, Application, Domain, Infrastructure, Auth, UI hosts, Shared client code, migration tooling, and tests. Refactors must preserve the good parts of that structure while correcting dependency direction, configuration hygiene, migration/seeding clarity, and shared-client/UI boundaries.

## Primary Goals

When working in this repository, optimize for these outcomes in this order:

1. Correct dependency direction and composition-root placement.
2. Preserve external behavior unless the task explicitly allows behavior changes.
3. Reduce hardcoded environment and runtime assumptions.
4. Keep changes small, reviewable, and reversible.
5. Improve clarity of code, naming, and documentation only when it supports architecture or maintainability.

## Non-Goals

Do not introduce broad rewrites, cosmetic churn, or speculative abstractions without a clear architectural payoff.

Do not convert the codebase into a different architecture style just because a pattern is fashionable.

Do not create AI-generated busywork, duplicate docs, or redundant helper layers.

## Project-Specific Expectations

### Dependency Direction

Preferred direction:

- Domain -> no dependency on API, Infrastructure, UI hosts, EF Core, ASP.NET transport concerns
- Application -> depends on Domain and application contracts only
- Infrastructure -> implements persistence and integration details
- Auth -> should depend on abstractions/contracts, not directly force Infrastructure into upper layers
- API -> composition root + HTTP transport only
- Migration tooling -> should not depend on API-specific service wiring
- Web/Desktop/Mobile -> host-specific composition and platform adapters only
- Shared client/UI -> should not become a junk drawer for every concern

### Configuration

Prefer configuration from:

1. environment variables
2. appsettings per environment
3. strongly typed options

Avoid:

- hardcoded localhost URLs
- hardcoded CORS origins
- hardcoded connection strings in code
- secrets committed into code or config templates

### Refactor Behavior

For refactor tasks:

- preserve runtime behavior unless explicitly told otherwise
- move one architectural slice at a time
- do not combine architecture cleanup, UI redesign, CI work, and deployment changes in a single task
- keep public surface changes minimal

### Tests and Validation

After code changes, run the smallest relevant validation first, then broader validation if needed.

Preferred commands from repo root:

```powershell
dotnet build OakERP.sln
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj
```

If a task touches only a specific project, start with a targeted build/test for that project first.

If validation cannot run, explain exactly why.

### Test Creation Expectation

When a task introduces new business behavior, new orchestration logic, a new seam, or a new document flow, add or update tests as part of the same task.

Default expectation:
- add/update **unit tests** for local business/orchestration behavior
- add/update **integration tests** when the change affects runtime wiring, persistence, API behavior, posting flow, or transaction boundaries

Do not treat “build passes” as sufficient when behavior changed.

If tests are intentionally deferred, say exactly why in `progress.md`.

## Planning Rules

For any task that touches multiple projects, changes architecture, or affects more than roughly 3 files, create and maintain task files under:

`docs/ai/tasks/active/<task-slug>/`

Use:

- `task_plan.md`
- `findings.md`
- `progress.md`

Minimum expectations:

- `task_plan.md`: scope, constraints, ordered steps, validation plan
- `findings.md`: current-state observations, dependency violations, risks, unknowns
- `progress.md`: what changed, what passed, what failed, what remains

Archive completed task folders under:

`docs/ai/tasks/archive/`

## What To Flag Before Editing

Stop and call out the issue before editing if you find:

- Domain depending directly on framework or infrastructure details
- API owning service registration that should live elsewhere
- migration or seeding behavior spread across too many paths
- tests that depend on hidden machine-specific assumptions
- UI/shared code taking on platform-specific behavior without a proper boundary
- committed secrets or production-like credentials

## Current Cleanup Priorities

Prioritize in this order unless the task says otherwise:

1. Move composition/service registration out of API-specific wiring where possible.
2. Reduce or remove `MigrationTool -> API` dependency.
3. Externalize client/API URLs, CORS settings, and test DB assumptions.
4. Simplify migration + seeding paths so each environment has one clear story.
5. Standardize test architecture and remove brittle setup.
6. Split shared client concerns from shared UI concerns.

## Documentation Rules

When adding docs:

- keep them concise and operational
- prefer repo-specific instructions over generic framework tutorials
- update architecture docs when structural rules change
- avoid duplicating the same guidance in multiple places

## Safety Rails

Do not:

- silently delete projects or folders
- introduce secrets
- rewrite large areas without a scoped plan
- change Docker, migration flow, or deployment behavior unless the task explicitly asks for it
- add new dependencies without explaining why they are needed
