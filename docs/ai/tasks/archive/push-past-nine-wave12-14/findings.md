# Findings

## Current-State Observations

- The AP and AR create/allocation workflows were still duplicating precondition loading, transaction control, settlement orchestration, and persistence-failure translation even after earlier service thinning.
- Desktop and Mobile were carrying copy-equivalent MAUI `ITokenStore`, `IPlatformService`, and `IFormFactor` implementations.
- `OakERP.Shared` cannot compile MAUI runtime APIs directly because it targets `net9.0`, so shared MAUI host-core code needs to be source-shared or moved into a MAUI-capable assembly.
- `ApInvoiceCreateWorkflow` and `AuthRegistrationWorkflow` were still manually coordinating transaction begin, rollback, save, and commit behavior.

## Risks

- Shared workflow helpers must not hide real AP vs AR business-rule differences.
- Shared MAUI source must not introduce a Web or Shared project target-framework problem.
- New helper abstractions should stay internal and small to avoid recreating the abstraction sprawl already removed in earlier waves.

## Final Decision Notes

- Shared MAUI adapters were implemented as linked source files physically stored under `OakERP.Shared/Hosts/Maui` and compiled into Desktop and Mobile only.
- `OakERP.Shared.csproj` explicitly excludes `Hosts/Maui/**/*.cs` from its own compile items so Web and shared Razor builds stay `net9.0`-safe.

## Deferred Unless Forced

- No further Web adapter consolidation beyond keeping Web-specific behavior separate.
- No further DTO or API contract cleanup in this slice.
- No change to schema, migrations, or posting runtime behavior.
