# OakERP PR Workflow Skill

## Purpose

Use this skill when Codex is preparing a branch for review, validating a PR locally, or creating/updating a GitHub PR for OakERP.

## What To Read

Read:
- `AGENTS.md`
- `docs/ai/codex-workflow.md`
- `docs/architecture/dependency-rules.md`
- `docs/architecture/project-map.md`

## Required Behavior

- Follow the branch, worktree, validation, and PR rules in `docs/ai/codex-workflow.md`.
- Prefer `pwsh ./tools/validate-pr.ps1` for local pre-PR validation.
- If task docs exist, keep them current before treating the PR as ready.
- Record deferred risks and missing external setup explicitly.

## Notes

- Keep this skill thin; the workflow doc is the source of truth.
- Do not commit MCP config, tokens, or other machine-specific secrets.
