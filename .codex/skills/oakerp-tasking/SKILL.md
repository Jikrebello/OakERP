# OakERP Tasking Skill

## Purpose
Use this skill when starting any non-trivial OakERP task that touches multiple files, multiple projects, architecture, configuration, tests, or process/tooling concerns.

## When To Use It
Use task files for work such as:
- architecture cleanup
- dependency-direction changes
- configuration cleanup
- migration/seeding changes
- test architecture changes
- client/UI boundary changes
- CI/process/tooling work
- anything likely to take more than one iteration

Do not use task files for tiny one-file typo fixes or isolated mechanical edits.

## Task Folder Convention
Create one folder per task under:

`docs/ai/tasks/active/<task-name>/`

Each task folder should contain:
- `task_plan.md`
- `findings.md`
- `progress.md`

Completed tasks can later be moved to:

`docs/ai/tasks/archive/`

## Preferred Way To Create A Task
Use the repo utility script:

`.\tools\new-codex-task.ps1 <task-name>`

Example:

`.\tools\new-codex-task.ps1 shared-ui-shell-cleanup`

## Expectations
For any task folder:
- `task_plan.md` defines scope, constraints, success criteria, and validation.
- `findings.md` captures current-state observations and dependency notes.
- `progress.md` records what changed, what was validated, and what remains.

## Working Style
1. Read `AGENTS.md`.
2. Read relevant architecture docs in `docs/architecture/`.
3. Create or update the task folder.
4. Audit before editing.
5. Make the smallest coherent change set.
6. Validate after changes.
7. Record results in `progress.md`.

## Guardrails
- Do not let task files become fiction. Keep them updated.
- Do not broaden a task silently.
- If new dependency conflicts appear, record them in `findings.md` before widening scope.
- Prefer one safe slice over one ambitious rewrite.
- Generated artifacts like `project-structure.txt` are navigation aids, not architecture docs.

## OakERP-Specific Notes
- Prefer dependency-direction fixes over cosmetic reorganization.
- Preserve behavior unless explicitly told otherwise.
- Avoid dragging Mobile into unrelated refactors.
- Avoid changing schema, migrations, or Identity inheritance unless the task explicitly calls for it.