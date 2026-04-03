# Progress

## Task
serena-setup

## Started
2026-03-30

## Work Log
- Created the task docs for the minimal Serena repo-side setup slice.
- Confirmed the repo was clean and had no existing `.serena` config or Serena task folder.
- Confirmed the current Codex workflow already treats MCP/client setup as external machine configuration.
- Added the planned repo-side Serena config and workflow updates.
- Kept `.serena/project.yml` intentionally small and repo-specific: project name, `csharp` language, gitignore-aware behavior, and one short prompt pointing Serena back to existing repo guidance.
- Added `.gitignore` rules so only `.serena/project.yml` is repo-tracked while local Serena files remain uncommitted.
- Replaced the earlier "no Serena-specific workflow yet" note with a short operational section in `docs/ai/codex-workflow.md` that keeps Serena optional and subordinate to the existing Codex workflow.
- Cleaned up an accidental local Serena expansion of `.serena/project.yml` before publish so the repo copy stays minimal.
- Removed a generated `project-structure.txt` drift that had captured local-only Serena cache/memory files and was out of scope for this slice.

## Files Touched
- `docs/ai/tasks/active/serena-setup/task_plan.md`
- `docs/ai/tasks/active/serena-setup/findings.md`
- `docs/ai/tasks/active/serena-setup/progress.md`
- `.serena/project.yml`
- `.gitignore`
- `docs/ai/codex-workflow.md`

## Validation
- `git diff --check`
  - Passed for patch structure; Git emitted existing line-ending warnings because the repo normalizes text with LF while the Windows worktree uses CRLF on checkout.
- `'.serena/project.local.yml', '.serena/memories/example.md' | git check-ignore -v --stdin`
  - Confirmed both paths match the new `.serena/*` ignore rule.
- `git status --short .serena/project.yml`
  - Confirmed `.serena/project.yml` remains visible to git as the intended tracked file.
- `pwsh ./tools/validate-pr.ps1`
  - Passed.
  - Included `dotnet tool restore`, `dotnet csharpier check .`, targeted restore/build for API, and unit/integration test runs.
  - Existing API warning remained during validation:
    - `OakERP.API/Controllers/BaseController.cs(23,57): warning CS8629`

## Remaining
- No remaining repo-side changes in this approved slice.

## Deferred Smells / Risks
- Local Serena installation and Codex MCP configuration remain outside the repo.
- Serena onboarding output and memories remain intentionally uncommitted.
- Any Serena dashboard usage, `project.local.yml`, caches, indexes, logs, custom modes, or machine-specific language-server settings remain local-only.

## Local-Only Follow-Up
- Install `uv` / `uvx`.
- Add Serena to local Codex MCP config outside the repo.
- Run Serena project creation/indexing locally if desired.
- Keep any generated `.serena` contents other than `project.yml` uncommitted.
