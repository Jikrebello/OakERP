# Task Plan

## Task Name
oakerp-skill-pack

## Goal
Create a usable OakERP skill pack that desktop can discover, with valid `SKILL.md` frontmatter, UI metadata, and focused instructions for the repo's main architectural workflows.

## Background
OakERP already had three repo-local skill folders, but they were not valid discoverable skills because their `SKILL.md` files had no YAML frontmatter and no `agents/openai.yaml` metadata. The repo also lacked focused skills for posting, migration and seeding, auth boundaries, client and host boundaries, and OakERP-specific test harness work.

## Scope
- `.codex/skills/`
- `docs/ai/tasks/active/oakerp-skill-pack/`
- local mirror install under `$CODEX_HOME/skills/` for desktop discovery

## Out of Scope
- application runtime code
- production configuration
- migrations, schema, or seeding behavior
- CI and deployment flow

## Constraints
- Preserve behavior unless explicitly asked otherwise.
- Do not introduce secrets or hardcoded environment values.
- Keep the change set as small as possible.
- Prefer structural improvement over cosmetic churn.
- Add abstractions only when they solve a real coupling or duplication problem.
- Keep thin orchestrators thin and pure engines and calculators pure when that is part of the intended design.
- Review domain-significant magic numbers and strings.

## Success Criteria
- [x] The target issue is addressed
- [x] Relevant skill validation passes
- [x] Docs updated if needed
- [x] Remaining risks are documented
- [x] Deferred smells and risks recorded if intentionally left unresolved

## Planned Steps
1. Audit the existing OakERP skill folders and identify why they are not discoverable.
2. Create or update repo-local OakERP skills with valid frontmatter and focused workflow instructions.
3. Validate each skill folder and mirror the finished skills into `$CODEX_HOME/skills`.

## Validation Commands
```powershell
python "C:\Users\James\.codex\skills\.system\skill-creator\scripts\quick_validate.py" ".\.codex\skills\<skill-name>"
```

## Test Notes
Neither unit nor integration tests are required because no application runtime behavior changes. Skill-folder validation is required instead.

## Risks

- Desktop may require a restart before the new skills appear in the picker.
- The current session may not hot-load newly created skills even after local installation.

## Architecture Checks

- No runtime models or persisted entity models should change in this task.
- No business-significant literals should move outside skill documentation.
- No test claims should be made without validation output.
- No new abstraction should be introduced in runtime code.
- No orchestrator or engine behavior should change.

## Notes

- Repo-local skills are the source of truth.
- Installed copies in `$CODEX_HOME/skills` are only for local desktop discovery.
