# Findings

## Task
ar-invoice-posting-1a

## Current State
- Application posting seam already exists in `OakERP.Application` via `IPostingService`, `PostCommand`, and `PostResult`.
- Domain posting abstractions already exist in `OakERP.Domain.Posting`, including `IPostingEngine`, `IPostingRuleProvider`, `PostingEngineResult`, and AR invoice posting context models.
- No posting service, posting engine, GL settings provider, or posting rule provider is implemented yet.
- AR invoices and GL entries already have the fields needed for Slice 1A posting and traceability.
- AR invoice lines can reference items and tax rates, but there is no line location and no stock-posting support for this slice.

## Relevant Projects
- `OakERP.Application`
- `OakERP.Domain`
- `OakERP.Infrastructure`
- `OakERP.Tests.Unit`
- `OakERP.Tests.Integration`

## Dependency Observations
- API composes Infrastructure services through `OakERP.Infrastructure.Extensions.ServiceCollectionExtensions`, so Slice 1A service registration belongs there rather than in API-only wiring.
- Existing repository/unit-of-work boundaries are already in place and should be reused instead of depending directly on `ApplicationDbContext` from posting orchestration.

## Structural Problems
- The repo contains duplicated `PostingRule` / `PostingRuleLine` models in both `OakERP.Domain.Posting` and `OakERP.Domain.Entities.Posting`.
- Existing repository interfaces do not yet expose a tracked AR invoice graph for posting or an open-period lookup by date.
- No persisted posting-rule backend exists today; using a code-backed runtime rule is the smallest safe slice.
- Integration test infrastructure used synchronous DI scopes in a few helpers, which fails when a resolved service graph contains async-disposable services such as the EF-backed unit of work.

## Configuration / Environment Notes
- `GlPostingSettings` has no implementation yet; the most local fit for Slice 1A is an Infrastructure provider backed by `app_settings`.
- Existing appsettings still contain local development/test connection strings and secrets, but this slice should not widen into config cleanup.

## Testing Notes
- Baseline before implementation: `dotnet build OakERP.sln` passes.
- Baseline before implementation: `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --no-build` passes.
- Baseline before implementation: `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --no-build` passes.
- No posting-specific unit or integration tests currently exist.

## Open Questions
- None currently blocking after scope approval.
- Repository additions remain acceptable only if they stay entity-local and do not force a broader persistence refactor.

## Recommendation
Implement Slice 1A entirely in Infrastructure with thin orchestration over repositories and `IUnitOfWork`, a pure AR invoice GL posting engine, narrow repository query additions only where needed, and full unit/integration test coverage in the same change.

## Final Notes
- The approved narrow repository additions stayed within existing ownership and did not require a broader persistence refactor.
- Slice 1A uses only the runtime `OakERP.Domain.Posting` rule types and does not touch `OakERP.Domain.Entities.Posting`.

