# Task Plan

## Task Name
architecture-remediation-wave1

## Goal
Implement the first aggressive architecture remediation wave by moving use-case orchestration into `OakERP.Application`, reducing framework and config leakage, unifying host API client registration, and adding architecture enforcement tests.

## Background
OakERP already has a workable project split, but orchestration still lives mostly in Infrastructure, Identity types still leak into Domain, host/config wiring is inconsistent, and architecture rules are documented more strongly than they are enforced.

## Scope
- `OakERP.Application`
- `OakERP.Auth`
- `OakERP.Domain`
- `OakERP.Infrastructure`
- `OakERP.API`
- `OakERP.Web`
- `OakERP.Desktop`
- `OakERP.Client`
- `OakERP.Shared`
- `OakERP.Tests.Unit`
- `OakERP.Tests.Integration`

## Out of Scope
- schema redesign
- new HTTP endpoints or contract changes
- large UI redesign
- generated migration cleanup beyond any changes needed to keep the solution compiling

## Constraints
- Preserve runtime behavior unless an externalized config fallback must be removed.
- Keep the public service interfaces stable for controllers and existing callers.
- Do not reintroduce `MigrationTool -> API` coupling.
- Keep Mobile changes minimal unless needed for build consistency.

## Success Criteria
- [ ] Application owns AP invoice, AP payment, AR receipt, and posting orchestration
- [ ] Domain no longer contains the ASP.NET Identity user type
- [ ] JWT and API client config use typed options instead of raw configuration reads
- [ ] Web/Desktop host API client wiring is unified
- [ ] Architecture tests enforce dependency direction rules
- [ ] Unit tests and targeted integration tests pass
- [ ] Solution build passes
- [ ] Deferred risks are documented

## Planned Steps
1. Move orchestration, validators, snapshot shaping, and supporting dependency bundles into `OakERP.Application`.
2. Relocate the Identity-backed `ApplicationUser` type out of Domain and update auth/persistence wiring.
3. Introduce typed options for JWT, API client, and integration test DB configuration; remove code-level fallback secrets.
4. Replace shared host service aggregation with explicit host composition and shared API client registration.
5. Add architecture tests and update task docs with outcomes and deferred risks.

## Validation Commands
```powershell
dotnet build OakERP.Application/OakERP.Application.csproj
dotnet build OakERP.Auth/OakERP.Auth.csproj
dotnet build OakERP.Infrastructure/OakERP.Infrastructure.csproj
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj
dotnet build OakERP.sln
```

## Test Notes
- unit tests: yes
- integration tests: yes, targeted after host/config changes

## Risks
- Identity type relocation may require limited migration snapshot cleanup to keep EF tooling stable.
- Config externalization may expose previously hidden local environment assumptions.
