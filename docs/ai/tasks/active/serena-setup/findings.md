# Findings

## Task
serena-setup

## Current State
OakERP already has the repo-side foundations Serena should build on: `AGENTS.md`, architecture docs, a Codex workflow doc, tasking templates, and backend-focused validation. The repo currently has no `.serena` folder, no Serena task folder, and no repo-committed Serena config.

## Relevant Areas
- repo root docs/config
- `.serena`
- `docs/ai`

## Dependency Observations
- No application dependency or architecture boundary changes are needed for this slice.
- Serena should stay a tooling aid for symbol-aware work, not a new owner of planning, validation, or PR flow.

## Structural Notes
- `docs/ai/codex-workflow.md` already says MCP installation/auth stays external machine setup.
- The previous workflow guidance explicitly deferred Serena until after the Codex workflow existed; that prerequisite is now met.
- The repo should track at most one Serena file in this slice: `.serena/project.yml`.

## Configuration / Environment Notes
- The machine already has `pwsh 7.6.0`, `.NET 10.0.201`, and `git config core.autocrlf=true`, which align with Serena’s documented C# and Windows prerequisites.
- `uv`/`uvx` is not installed locally yet.
- No local `~/.serena` home currently exists.
- A local `~/.codex/config.toml` exists, but Serena is not configured there yet.

## Testing Notes
- No build/test run is required for this docs/config-only slice.
- The main validation need is confirming `.gitignore` behavior for tracked vs local-only Serena files.

## Open Questions
- None blocking this approved minimal slice.

## Deferred Smells / Risks
- Installing Serena and `uv`, adding Codex MCP client config, and running Serena onboarding remain local-only follow-up.
- Any shared Serena memories, custom modes, JetBrains backend setup, or CI integration are intentionally deferred.

## Recommendation
Commit a tiny `.serena/project.yml`, ignore the rest of `.serena`, add a short Serena note to the Codex workflow doc, and document the remaining local-only setup explicitly.
