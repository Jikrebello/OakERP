name: Feature
about: Track a new feature or capability
title: "feature: "
labels: ["feature"]
assignees: []

## Summary
What feature is being requested?

## Why
What problem does this solve?

## Desired Outcome
What should be true when this is done?

## Affected Area
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
- [ ] CI / tooling

## Scope
What should be included?

## Out of Scope
What should explicitly not be included?

## Constraints
What must stay unchanged?

- [ ] public API behavior
- [ ] schema / migrations
- [ ] auth flow behavior
- [ ] routes / pages / layouts
- [ ] test behavior
- [ ] other

Notes:

## Risks / Dependencies
What does this depend on, or what could make it risky?

## Validation
How will this be verified?

```text
dotnet build OakERP.sln
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --no-restore
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --no-restore
````

## Related Task Folder

If this is being worked as a Codex task, link:

`docs/ai/tasks/active/<task-name>/`