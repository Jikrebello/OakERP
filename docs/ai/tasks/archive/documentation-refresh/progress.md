# Progress

## Started

- Audited the root README, dev docs hub, operational docs, and architecture docs.
- Confirmed the current runtime ports, test frameworks, API result/exception behavior, and host split.

## Completed

- Rewrote the root `README.md` around the real solution structure, current implemented slices, and current doc entrypoints.
- Turned `OakERP.Docs/README-dev.md` into the real local developer onboarding page.
- Refreshed:
  - `OakERP.Docs/db-setup.md`
  - `OakERP.Docs/ef-core.md`
  - `OakERP.Docs/oakERP-testing-guide.md`
- Added:
  - `OakERP.Docs/api-conventions.md`
  - `OakERP.Docs/architecture-overview.md`
- Updated `docs/architecture/project-map.md` to reflect current API transport ownership and shared MAUI host-core ownership.

## Validation

- searched the living/public docs for stale references:
  - `/OakERP.Solution`
  - `5432:5432`
  - `OakERP.WebAPI`
  - obsolete public status language
- verified the refreshed docs point at existing files for:
  - developer docs hub
  - DB setup
  - HTTPS certificate setup
  - EF Core helper
  - testing guide
  - API conventions
  - architecture overview
  - dependency rules
  - project map

## Notes

- `docs/ai/tasks/...` history was intentionally left untouched.
- generated tree/map artifacts were intentionally left untouched for this wave.
