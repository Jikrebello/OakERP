name: Refactor
about: Track an architecture, cleanup, or structural refactor task
title: "refactor: "
labels: ["refactor"]
assignees: []

## Summary
What is being cleaned up or refactored?

## Why
Why does this need to be done?

## Problem Type
- [ ] dependency direction
- [ ] composition / DI
- [ ] configuration
- [ ] migration / seeding
- [ ] auth / identity
- [ ] client plumbing
- [ ] shared UI / view-models
- [ ] tests / harness
- [ ] CI / tooling
- [ ] docs / tasking
- [ ] other

## Current State
What is wrong today?

## Target State
What should be true after this refactor?

## Scope
Which projects/files are likely involved?

- [ ] OakERP.API
- [ ] OakERP.Application
- [ ] OakERP.Auth
- [ ] OakERP.Client
- [ ] OakERP.Common
- [ ] OakERP.Desktop
- [ ] OakERP.Domain
- [ ] OakERP.Infrastructure
- [ ] OakERP.MigrationTool
- [ ] OakERP.Mobile
- [ ] OakERP.Shared
- [ ] OakERP.Tests.Integration
- [ ] OakERP.Tests.Unit
- [ ] OakERP.UI
- [ ] OakERP.Web
- [ ] docs
- [ ] tooling / scripts

## Constraints
What must stay unchanged?

- [ ] public API behavior
- [ ] schema / migrations
- [ ] auth flow behavior
- [ ] routes / pages / layouts
- [ ] test behavior
- [ ] other

Notes:

## Risks
What could go wrong?

## Validation
How will this be verified?

```text
dotnet build OakERP.sln
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --no-restore
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --no-restore
```

## Task Folder

If this is a Codex-driven task, link the folder:

```
docs/ai/tasks/active/<task-name>/
```

## Definition of Done

-  scope is clear
-  implementation plan exists
-  build passes
-  relevant tests pass
-  docs/task files updated
-  deferred risks recorded