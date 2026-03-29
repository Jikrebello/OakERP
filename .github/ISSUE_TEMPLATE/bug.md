name: Bug
about: Report a defect or regression
title: "bug: "
labels: ["bug"]
assignees: []

## Summary
What is broken?

## Expected Behavior
What should have happened?

## Actual Behavior
What happened instead?

## Where
Which area is affected?

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

## Steps To Reproduce
1.
2.
3.

## Environment
- [ ] Development
- [ ] Testing
- [ ] Production
- [ ] CI
- [ ] Local machine

Notes:

## Logs / Errors / Evidence
Paste stack traces, logs, screenshots, or request details here.

## Regression?
- [ ] Yes
- [ ] No
- [ ] Unknown

If yes, what changed?

## Severity
- [ ] low
- [ ] medium
- [ ] high
- [ ] critical

## Validation
How will the fix be verified?

```text
dotnet build OakERP.sln
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --no-restore
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --no-restore
````

## Related Task Folder

If this is being worked as a Codex task, link:

`docs/ai/tasks/active/<task-name>/`