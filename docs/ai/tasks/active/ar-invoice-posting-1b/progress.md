# Progress

## Task
ar-invoice-posting-1b

## Started
2026-03-29

## Work Log
- Created Slice 1B task docs and recorded scope and risks before editing code.
- Added `ArInvoiceLine.LocationId` / `Location`, configuration, and one migration for `ar_invoice_lines.location_id`.
- Added prepared per-line posting context and an AR invoice posting context builder so stock-line validation, account resolution, and moving-average costing happen outside the engine.
- Implemented `MovingAverageInventoryCostService` using historical `inventory_ledgers` ordered by `TrxDate`, `CreatedAt`, then `Id`.
- Extended the pure AR invoice posting engine and code-backed runtime rule to emit COGS, inventory asset, and inventory movement effects for stock lines.
- Updated `PostingService` to persist inventory ledger rows in the same transaction and preserve existing non-stock behavior.
- Expanded unit tests for the engine, context builder, and moving-average cost service.
- Reworked AR invoice integration tests to cover non-stock preservation, stock success, mixed invoices, negative-stock posting, duplicate-post resistance, and transactional stock failures.
- Updated the integration test base to apply migrations before seeding so the reset database reflects the new schema.

## Validation
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj`
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ArInvoicePosting`
- `dotnet build OakERP.sln`

## Remaining
- None within Slice 1B scope.

## Outcome
- Completed.
