# Findings

## Task
oakerp-skill-pack

## Current State
OakERP already includes repo-local skills under `.codex/skills`, but the three existing skills are not valid discoverable skills because they contain plain markdown only. They are missing the required YAML frontmatter and they do not provide any `agents/openai.yaml` UI metadata.

## Relevant Projects
- `.codex/skills`
- `docs/architecture`
- `docs/ai/tasks/archive`

## Dependency Observations
- No application-layer dependencies are affected because this task is documentation and skill metadata only.
- Desktop discoverability appears to rely on valid skill structure and local skill installation under `$CODEX_HOME/skills`.

## Structural Problems
- Existing skills cover only broad architecture, tasking, and PR workflow concerns.
- There are no focused skills for posting, migration and seeding, auth seams, client and host boundaries, or test harness work.
- Existing skill folders were likely invisible to desktop because they were never initialized as proper skills.

## Literal / Model-Family Notes
- Repeated business-significant literals: not applicable for runtime code in this slice
- Repeated domain-significant numbers: none introduced
- Runtime-vs-persisted model-family conflicts: not applicable
- Thin orchestrators getting too thick: not applicable
- Pure engines and calculators with side effects or lookups: not applicable

## Configuration / Environment Notes
- `$CODEX_HOME` resolves to `C:\Users\James\.codex`.
- Local desktop discovery may require copying the finished skills into `$CODEX_HOME/skills` and restarting Codex.

## Testing Notes
- Runtime code is untouched, so unit and integration tests are not required for this slice.
- Skill-folder validation should be run for every created or updated skill.

## Rollback / Transaction Notes
- Migration rollback reviewed: not applicable
- Transactional failure leaves no writes: not applicable

## Open Questions
- Whether repo-local `.codex/skills` hot-load in the current desktop session remains unclear.

## Deferred Smells / Risks
- If desktop still does not surface repo-local skills after restart, the product may currently prefer installed skills over project-local skills in this environment.

## Recommendation
Make the existing skills valid, add a focused OakERP skill pack around the repo's main workflows, validate each folder, and mirror the completed skills into `$CODEX_HOME/skills` for desktop pickup.
