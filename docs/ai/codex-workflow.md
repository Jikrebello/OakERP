# Codex Workflow

## Purpose

This document is the lightweight OakERP workflow for Codex-driven branch, validation, and pull request work.

Use it alongside:
- `AGENTS.md`
- `docs/architecture/dependency-rules.md`
- `docs/architecture/project-map.md`
- `.codex/skills/oakerp-architecture/SKILL.md`
- `.codex/skills/oakerp-tasking/SKILL.md`

## Branch And Worktree Defaults

- Prefer one coherent task per branch.
- For new Codex-created branches, prefer `codex/<task-slug>`.
- Keep long-lived `feature/...` branches exceptional rather than the default Codex path.
- For parallel Codex work, prefer a separate git worktree per task rather than sharing one worktree across multiple active editing threads.

Recommended worktree flow from the parent folder:

```powershell
git fetch origin
git worktree add ..\OakERP-<task-slug> -b codex/<task-slug> origin/main
```

## Tasking Expectation

- If the task touches multiple files, multiple projects, architecture, tests, CI, or tooling, create/update a task folder under `docs/ai/tasks/active/<task-slug>/`.
- Keep `task_plan.md`, `findings.md`, and `progress.md` current.
- Record deferred smells, risks, and missing external setup explicitly rather than implying they were fixed.

## Local Validation

Before opening or updating a PR, run:

```powershell
pwsh ./tools/validate-pr.ps1
```

This script is intentionally aligned with the current backend CI checks:
- `dotnet tool restore`
- `dotnet csharpier check .`
- `dotnet restore OakERP.API/OakERP.API.csproj`
- `dotnet restore OakERP.Tests.Unit/OakERP.Tests.Unit.csproj`
- `dotnet restore OakERP.Tests.Integration/OakERP.Tests.Integration.csproj`
- `dotnet build OakERP.API/OakERP.API.csproj --no-restore`
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --no-restore`
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --no-restore`

The restore stays backend-targeted on purpose so GitHub CI does not fail on MAUI workloads from host projects that this job does not build or test.

Integration-test note:
- The script assumes the existing OakERP test database prerequisites are already available locally.
- If integration tests cannot run, record the exact reason in `progress.md` and in the PR.

## Pull Request Flow

- Open a **Draft PR** first for any non-trivial Codex branch.
- Fill the PR template completely.
- Include the task folder path if one exists.
- List the exact validation commands you ran.
- Call out any intended behavior change.
- Record deferred smells, risks, or external follow-up clearly.

A PR is ready to move out of draft only when:
- the scoped task docs are current
- local validation has been run or any gap is explicitly explained
- the PR summary matches the actual change set
- deferred risks are listed plainly

## GitHub And MCP Assumptions

- GitHub PR creation/review depends on working GitHub connector auth outside the repo.
- MCP server installation and authentication are external machine setup, not repo-tracked configuration.
- Do not commit MCP config, tokens, or machine-specific secrets into this repo.
- If MCP or GitHub access is unavailable, continue with local branch/task/doc hygiene and call out the missing external setup.

## What This Repo Deliberately Does Not Do Yet

- No repo-committed MCP config
- No CODEOWNERS
- No heavy PR automation beyond the current backend CI
- No Serena-specific workflow yet

Land one clean Codex-driven PR cycle with this workflow before introducing Serena.
