# Documentation Refresh

## Scope

- Refresh the living/public documentation only.
- Update `README.md`, `OakERP.Docs`, and `docs/architecture`.
- Leave `docs/ai/tasks/...` historical records and generated tree/map artifacts untouched.

## Constraints

- Documentation-only task.
- Keep the root README balanced: product summary first, concrete engineering guidance immediately after.
- Match current project names, ownership boundaries, ports, URLs, and setup flow exactly.

## Ordered Steps

1. Rewrite the root `README.md` around the real solution structure and current implemented slices.
2. Upgrade `OakERP.Docs/README-dev.md` into the actual developer onboarding entrypoint.
3. Refresh stale operational docs in `OakERP.Docs`:
   - `db-setup.md`
   - `ef-core.md`
   - `oakERP-testing-guide.md`
4. Add:
   - `OakERP.Docs/api-conventions.md`
   - `OakERP.Docs/architecture-overview.md`
5. Do a narrow consistency sweep in `docs/architecture/project-map.md`.
6. Validate links and search for the known stale references.

## Validation

- Verify internal Markdown links point to existing files.
- Search for stale references:
  - `README-dev.md` as a root file
  - `/OakERP.Solution`
  - `5432:5432` in current dev DB instructions
  - `OakERP.WebAPI`
  - obsolete status language in the living/public docs
