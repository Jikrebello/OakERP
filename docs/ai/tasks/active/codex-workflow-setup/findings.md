# Findings

## Task
codex-workflow-setup

## Current State
OakERP already has a GitHub PR template, backend CI, Codex architecture/tasking skills, and a task scaffold script. The current branch tracks `origin`, and GitHub connector access is available from the environment. What is missing is a repo-local Codex workflow doc, a local PR validation script, a pinned SDK, and a thin workflow skill for repeatable PR work.

## Relevant Projects
- repo root docs/tooling
- `.github`
- `.codex`

## Dependency Observations
- No application dependency changes are needed for this workflow slice.
- The existing backend CI checks are the right baseline for `validate-pr.ps1`.

## Structural Problems
- No repo-local documented branch/worktree/PR workflow for Codex-driven work.
- No repo-local one-command validation path that mirrors backend CI.
- No `global.json`, despite CI being pinned to `.NET 9`.
- No thin Codex PR workflow skill.
- Backend CI currently restores `OakERP.sln`, which pulls in MAUI host workloads on GitHub runners even though this job is meant to validate backend-only changes.

## Literal / Model-Family Notes
- Repeated business-significant literals:
- Repeated domain-significant numbers:
- Runtime-vs-persisted model-family conflicts:
- Thin orchestrators getting too thick:
- Pure engines/calculators with side effects or lookups:

## Configuration / Environment Notes
- CI uses `actions/setup-dotnet` with `9.0.x`.
- Local `dotnet --version` currently returns `10.0.201`, so repo-local SDK pinning is needed for consistency.
- Integration tests still rely on the existing test database setup and connection-string behavior.
- MCP auth and GitHub repo settings are external to the repo.

## Testing Notes
- No test code changes are in scope.
- The validation script itself should be run once after creation.
- Backend CI and the local validation script should restore only the backend projects they actually build and test.

## Rollback / Transaction Notes
- Migration rollback reviewed:
- Transactional failure leaves no writes:

## Open Questions
- None currently blocking this lightweight slice.

## Deferred Smells / Risks
- Branch protection, required checks, and squash-merge policy still need GitHub-side configuration.
- MCP server installation/auth and Serena setup remain outside the repo.

## Recommendation
Add the repo-local workflow doc, PR validation script, SDK pin, thin skill, and a stronger PR template now. Leave GitHub settings, MCP auth, and Serena for external follow-up.

Follow-up: narrow backend CI and `validate-pr.ps1` restore steps to the API and test projects instead of the full solution so PR checks do not depend on MAUI workloads.
