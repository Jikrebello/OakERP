# Progress

## Task
codex-workflow-setup

## Started
2026-03-29 16:25:00

## Work Log
- Created the task docs for the workflow setup slice.
- Confirmed current backend CI behavior, current PR template contents, and local SDK version before editing.
- Added a repo-local Codex workflow document for branch, worktree, validation, and PR expectations.
- Added `tools/validate-pr.ps1` aligned with the current backend CI checks.
- Added `global.json` pinned to `.NET 9` with feature-band roll-forward so local runs stay on the CI major/minor line.
- Updated the PR template with task link, scope statement, validation, behavior-change note, and deferred risk prompts.
- Added a thin `oakerp-pr-workflow` skill that points back to the workflow doc.
- Added a short `AGENTS.md` pointer to the workflow doc.

## Files Touched
- `docs/ai/tasks/active/codex-workflow-setup/task_plan.md`
- `docs/ai/tasks/active/codex-workflow-setup/findings.md`
- `docs/ai/tasks/active/codex-workflow-setup/progress.md`
- `docs/ai/codex-workflow.md`
- `tools/validate-pr.ps1`
- `global.json`
- `.github/PULL_REQUEST_TEMPLATE.md`
- `.codex/skills/oakerp-pr-workflow/SKILL.md`
- `AGENTS.md`

## Validation
- `dotnet --list-sdks` confirmed installed `.NET 9` SDKs are available locally
- `pwsh ./tools/validate-pr.ps1` passed
- The validation run used `.NET SDK 9.0.312` via `global.json` roll-forward
- Validation included:
  - `dotnet tool restore`
  - `dotnet csharpier check .`
  - `dotnet restore OakERP.sln`
  - `dotnet build OakERP.API/OakERP.API.csproj --no-restore`
  - `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --no-restore --verbosity normal`
  - `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --no-restore --verbosity normal`
- Existing API warning still appears during validation:
  - `OakERP.API/Controllers/BaseController.cs(23,57): warning CS8629`

## Remaining
- No remaining repo-side changes in this approved slice.

## Deferred Smells / Risks
- GitHub branch protection, required checks, and squash-merge policy still need GitHub-side configuration.
- GitHub/Codex connector auth and any non-default MCP server auth remain external machine setup.
- Serena should still wait until after one clean PR cycle using this workflow.

## Outcome
- Completed lightweight repo-side Codex workflow setup.

## Next Recommended Step
- Use this branch to open a Draft PR with the updated template and workflow, then handle GitHub-side settings and external MCP/Serena setup separately.
