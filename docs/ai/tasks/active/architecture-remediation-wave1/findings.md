# Findings

## Task
architecture-remediation-wave1

## Current State
- `OakERP.Application` is mostly contracts and DTOs.
- AP/AR/posting orchestration lives in `OakERP.Infrastructure`.
- `ApplicationUser` lives in `OakERP.Domain` and inherits `IdentityUser`.
- Web and Desktop hosts both compose client/auth/API services, but they do it differently.
- JWT and test DB configuration still use raw configuration access and fallback literals.

## Relevant Projects
- `OakERP.Application`
- `OakERP.Auth`
- `OakERP.Domain`
- `OakERP.Infrastructure`
- `OakERP.Web`
- `OakERP.Desktop`
- `OakERP.Client`
- `OakERP.Tests.Unit`
- `OakERP.Tests.Integration`

## Dependency Observations
- `OakERP.Application` currently does not reference `OakERP.Domain`, which blocks use-case orchestration from living there.
- `OakERP.Infrastructure` already owns repositories, posting builders, posting providers, DB context, and seeding, which is the correct lower-layer shape.
- `OakERP.MigrationTool` currently shares infrastructure bootstrapping without depending on API, which must be preserved.

## Structural Problems
- Workflow services in Infrastructure coordinate validation, loading, transaction boundaries, snapshot shaping, and persistence together.
- Host-side client registration is duplicated and partially hardcoded.
- Shared registration currently hides client/UI composition boundaries.
- Persistence-specific exception handling was embedded in orchestration code and needed to stay below the Application boundary.

## Literal / Model-Family Notes
- Framework-backed identity model currently sits in Domain.
- Hardcoded local/dev URLs and connection-string fallbacks still appear in runtime and test support code.

## Testing Notes
- Existing unit tests exercise AP/AR/posting/auth behavior and can be retargeted to the moved application services.
- Architecture rules are not currently enforced by tests.

## Open Questions
- EF model snapshots do not need churn in this wave because `ApplicationUser` kept its existing namespace while moving out of the Domain assembly.

## Deferred Smells / Risks
- Mobile remains less aligned than Web/Desktop and will stay minimal in this wave unless a shared registration change requires a host update.
- `ApplicationUser` now lives in `OakERP.Auth` while retaining the `OakERP.Domain.Entities.Users` namespace as a staged compatibility move; a later wave can normalize the namespace once snapshot churn is acceptable.
