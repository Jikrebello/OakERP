# Findings

## Task
ar-invoice-posting-1b

## Current State
- Slice 1A already posts AR invoices through `IPostingService` with GL-only effects.
- `PostingEngineResult` already supports both GL entries and inventory movements.
- `IInventoryCostService` already exists as a domain posting seam but has no implementation.
- `ArInvoiceLine` does not currently carry location, so stock posting cannot be completed correctly without the approved schema change.
- `IInventoryLedgerRepository` already exposes `QueryNoTracking()` and `AddAsync()`, which are sufficient for the approved 1B cost-read and write path.

## Architecture Notes
- The current AR invoice repository load can be widened locally to include `ArInvoiceLine.Location` without changing repository ownership.
- The cost-resolution policy must not use `v_item_balance`, because it is current-state only and not safe for backdated posting.
- A prepared line context is needed so the engine stays pure and deterministic.
- The integration test reset path uses Respawn only, so applying a new schema change required migrating the test database at setup before seeding.

## Risks
- Same-date historical costing becomes nondeterministic unless read ordering is fixed explicitly.
- Allowing negative stock is an intentional policy for 1B and should be documented in tests and task notes.
- Missing prior cost basis must fail before any persistence writes occur.
- EF scaffolding surfaced unrelated pre-existing snapshot drift while generating the migration; the actual migration file was trimmed back to the approved `ArInvoiceLine.LocationId` change only.
