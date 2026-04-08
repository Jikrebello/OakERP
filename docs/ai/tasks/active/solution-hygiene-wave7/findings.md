# Findings

## Current-State Observations
- workflow and posting failures were still represented as `InvalidOperationException` / `NotSupportedException` with ad-hoc strings
- API exception handling collapsed almost all failures into a generic 500 response
- Auth/AP/AR result failures repeated hard-coded messages and persistence constraint names across validators and services
- Application workflows still pulled time directly from `DateTimeOffset.UtcNow`
- posting rounding policy lived inline instead of in one accounting-policy location
- dev/prod appsettings still contained committed connection strings, JWT keys, localhost URLs, and seed credentials

## Risks
- top-level `Program` configuration validation runs before `WebApplicationFactory` can add in-memory test settings
- moving all environments to fully blank config would break the `Testing` environment boot path

## Deferred / Intentional Debt
- `OakERP.API/appsettings.Testing.json` keeps explicit test-only startup values so integration tests can bootstrap under `WebApplicationFactory`; dev/prod defaults remain externalized
- this wave does not introduce localization or a repo-wide error-code catalog; errors are centralized by workflow slice only
