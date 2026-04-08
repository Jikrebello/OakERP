# Findings

## Current State

- `ApplicationUser` is physically defined in `OakERP.Auth/ApplicationUser.cs` but still declares `namespace OakERP.Domain.Entities.Users`.
- Auth, infrastructure, and test code still import `ApplicationUser` through the old domain namespace.
- EF snapshot/designer metadata is pinned to the old CLR type string `OakERP.Domain.Entities.Users.ApplicationUser`.
- `Tenant` and `License` remain legitimate Domain entities and should stay there in this wave.

## Risks

- EF metadata drift is the main risk; designer/snapshot files must stay consistent with the renamed CLR type without changing schema.
- Auth tests and seeding code construct `ApplicationUser` directly, so namespace updates must be compile-clean across auth and infrastructure.
- The repo already has uncommitted wave-3 posting changes; this wave must avoid touching that slice.

## Desired Shape

- `ApplicationUser` is declared in `OakERP.Auth`.
- No code outside Auth refers to `OakERP.Domain.Entities.Users.ApplicationUser`.
- `Tenant` and `License` remain under `OakERP.Domain.Entities.Users`.
- EF tracked metadata consistently points to `OakERP.Auth.ApplicationUser`.
