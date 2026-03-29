# Task Plan

## Task Name
codex-workflow-setup

## Goal
Add the lightweight repo-side workflow pieces needed for repeatable Codex-driven branch, validation, and PR work without changing application behavior.

## Background
OakERP already has Codex architecture/tasking guidance, a PR template, and backend CI, but it does not yet have a single repo-local workflow doc, a local PR validation script, a pinned SDK, or a thin Codex PR workflow skill.

## Scope
- `AGENTS.md`
- `.github/PULL_REQUEST_TEMPLATE.md`
- `docs/ai/codex-workflow.md`
- `tools/validate-pr.ps1`
- `global.json`
- `.codex/skills/oakerp-pr-workflow/SKILL.md`
- task docs under `docs/ai/tasks/active/codex-workflow-setup/`

## Out of Scope
- application code
- tests
- CI behavior beyond staying aligned with the current backend CI checks
- repo-committed MCP config or secrets
- CODEOWNERS
- GitHub repo settings changes
- Serena setup

## Constraints
- Preserve behavior unless explicitly asked otherwise.
- Do not introduce secrets or hardcoded environment values.
- Keep the change set as small as possible.
- Prefer structural improvement over cosmetic churn.
- Add abstractions only when they solve a real coupling or duplication problem.
- Keep the workflow lightweight and practical.

## Success Criteria
- [ ] A repo-local Codex workflow doc exists and is OakERP-specific
- [ ] A repo-local PR validation script exists and matches current backend CI checks
- [ ] `global.json` is aligned with the current CI .NET major/minor version
- [ ] The PR template requires task link, scope, validation, behavior-change note, and deferred risks
- [ ] A thin Codex PR workflow skill exists and points to the workflow doc
- [ ] `AGENTS.md` includes a short pointer to the workflow doc
- [ ] Relevant build/tooling validation passes
- [ ] Deferred external setup items are documented

## Planned Steps
1. Create task docs and record the current repo workflow state.
2. Add the lightweight workflow doc, PR validation script, SDK pin, and thin skill.
3. Update the PR template and add a short `AGENTS.md` pointer.
4. Validate the new script and record any environment assumptions or remaining external setup.

## Validation Commands
```powershell
pwsh ./tools/validate-pr.ps1
dotnet --version
```

## Test Notes
Neither application nor test code changes are planned, but the new validation script should be executed to prove the repo-side workflow setup is usable.

## Risks

- Local integration-test execution still depends on the existing test database prerequisites.
- GitHub branch protection, required checks, and MCP authentication remain external configuration.

## Architecture Checks

- Is the workflow doc concrete and OakERP-specific rather than generic process theater?
- Is the validation script aligned with current backend CI rather than inventing a new pipeline?
- Are external configuration responsibilities clearly separated from repo-tracked setup?

## Notes

- Keep the skill thin and avoid duplicating the workflow doc in two places.
- Do not add CODEOWNERS or repo-committed MCP config in this slice.
