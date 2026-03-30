# Task Plan

## Task Name
serena-setup

## Goal
Add the smallest safe repo-side Serena setup for OakERP so future Codex work can opt into symbol-aware project navigation without adding repo clutter or new process.

## Background
OakERP already has repo-specific Codex workflow guidance, tasking discipline, architecture docs, and validation conventions. Serena should complement that existing setup, not replace it, and all client/MCP wiring must remain local machine setup.

## Scope
- `.serena/project.yml`
- `.gitignore`
- `docs/ai/codex-workflow.md`
- task docs under `docs/ai/tasks/active/serena-setup/`

## Out of Scope
- application code
- tests
- CI changes
- repo-committed MCP/client config
- committed Serena memories, dashboards, caches, or machine-specific paths
- architecture redesign

## Constraints
- Preserve behavior unless explicitly asked otherwise.
- Keep setup lightweight and repo-specific.
- Do not duplicate `AGENTS.md` or `docs/ai/codex-workflow.md` inside Serena config.
- Do not introduce secrets or hardcoded machine paths.
- Keep Serena subordinate to the existing Codex workflow.

## Success Criteria
- [ ] A minimal repo-tracked `.serena/project.yml` exists
- [ ] Generated Serena project data stays git-ignored except for the tracked config file
- [ ] `docs/ai/codex-workflow.md` has a short operational Serena note
- [ ] Task docs record scope, findings, validation, and local-only follow-up
- [ ] No repo-committed client/MCP config, memories, or machine-specific setup is added

## Planned Steps
1. Create the task docs and record the current Serena/Codex repo state.
2. Add the minimal `.serena/project.yml` and matching `.gitignore` rules.
3. Add a short Serena section to `docs/ai/codex-workflow.md`.
4. Validate tracked-vs-ignored behavior and record what remains local-only.

## Validation Commands
```powershell
git diff --check
git check-ignore -v .serena/project.local.yml
git check-ignore -v .serena/memories/example.md
git status --short
```

## Test Notes
This is a docs/config slice only. No application behavior changes or automated test changes are expected.

## Risks

- Local Serena usability still depends on external installation/configuration outside the repo.
- The repo-side config must stay small enough that it does not become a second workflow document.

## Architecture Checks

- Does Serena remain an optional tool rather than a new architecture/process owner?
- Are repo-tracked instructions still sourced from `AGENTS.md` and `docs/ai/codex-workflow.md`?
- Are machine-local concerns still kept out of the repo?

## Notes

- Serena memories and onboarding output should remain local-only in this slice.
