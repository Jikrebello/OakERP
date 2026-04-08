---
name: oakerp-test-harness
description: Targeted testing and harness work for OakERP. Use when adding or updating unit tests, integration tests, test factories, WebApiIntegrationTestBase, test database reset behavior, or validation scope for backend changes.
---

# OakERP Test Harness

Use this skill when a task needs OakERP-specific test coverage or test-harness changes.
Choose the smallest test surface that proves the behavior, then widen only when the change truly crosses runtime boundaries.

## Read First

Inspect:
- `AGENTS.md`
- `docs/ai/codex-workflow.md`
- `docs/ai/tasks/archive/test-harness-cleanup/progress.md`
- `OakERP.Tests.Unit`
- `OakERP.Tests.Integration`

Pay special attention to:
- `OakERP.Tests.Integration/TestSetup/WebApiIntegrationTestBase.cs`
- `OakERP.Tests.Integration/TestSetup/OakErpWebFactory.cs`
- `OakERP.Tests.Integration/TestSetup/TestConfiguration.cs`
- `OakERP.Tests.Integration/TestSetup/TestDatabaseReset.cs`

## Core Rules

- Treat build success as insufficient when behavior changes.
- Add or update unit tests for local business logic, seams, and orchestration behavior.
- Add or update integration tests when runtime wiring, persistence, API behavior, posting flow, or transaction boundaries change.
- Do not claim fallback coverage unless the setup truly exercises the fallback path.
- Run validation sequentially if parallel `dotnet` processes start causing file-lock noise.
- Record exact gaps in `progress.md` if integration prerequisites are missing.

## Workflow

### 1. Match The Change To The Test Surface

Use:
- unit tests for pure logic, builders, service seams, and local orchestration
- integration tests for API, persistence, DI wiring, transactions, and end-to-end posting flow
- both when a narrow logic change also changes runtime behavior boundaries

### 2. Keep Harness Changes Narrow

If the task touches the harness itself, prefer:
- removing dead paths
- clarifying configuration ownership
- tightening factories or helpers used by active tests

Avoid redesigning the full test framework unless the task explicitly asks for that.

### 3. Validate In Increasing Scope

Start targeted, then broaden only if needed:

```powershell
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj
dotnet build OakERP.sln
```

For PR-ready backend work, prefer:

```powershell
pwsh ./tools/validate-pr.ps1
```

## Review Checklist

- Did the tests prove the changed behavior instead of the setup?
- Did the harness change stay focused on active paths?
- Were integration prerequisites or failures documented exactly?
- Were validation commands run in the narrowest sensible order?
