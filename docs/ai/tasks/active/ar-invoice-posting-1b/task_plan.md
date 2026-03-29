# Task Plan

## Task Name
ar-invoice-posting-1b

## Goal
Extend the Slice 1A AR invoice posting path to support stock lines with location-based inventory movements and COGS / inventory asset GL effects, while preserving the existing non-stock posting behavior.

## Scope
- `OakERP.Domain` posting context models and AR invoice line entity shape
- `OakERP.Infrastructure` posting services, cost resolution, repository load widening, configuration, and one migration
- `OakERP.Tests.Unit`
- `OakERP.Tests.Integration`
- `docs/ai/tasks/active/ar-invoice-posting-1b/`

## Constraints
- Keep the seam unchanged: `IPostingService` / `PostCommand`
- Keep the only schema change to `ArInvoiceLine.LocationId` / `Location`
- Keep `ArInvoicePostingEngine` pure and deterministic
- Put stock-line preparation, account resolution, and cost resolution outside the engine
- No AP / receipts / payments
- No unposting
- No auth / identity / UI work
- No broad rules-engine redesign
- Preserve Slice 1A non-stock behavior
- If repository widening would force a broader redesign, stop and report instead of broadening

## Success Criteria
- [x] Stock AR invoice lines can be posted through the existing posting seam
- [x] Non-stock invoices keep existing Slice 1A behavior
- [x] Stock lines require `LocationId`
- [x] Posting writes balanced GL plus inventory movement rows transactionally
- [x] Moving-average costing is resolved outside the engine using deterministic historical ordering
- [x] Failure paths leave invoice, GL, and inventory tables unchanged
- [x] Unit and integration tests cover the new stock behavior in the same slice

## Planned Steps
1. Create and update task docs.
2. Add the narrow schema/entity/configuration change for `ArInvoiceLine.LocationId`.
3. Extend the posting context with prepared per-line facts and add a narrow context builder plus moving-average cost service.
4. Update `PostingService`, `ArInvoicePostingEngine`, and the runtime rule provider for stock-line posting.
5. Add and update unit/integration tests.
6. Run validation and record outcomes.

## Validation Commands
```powershell
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ArInvoicePosting
dotnet build OakERP.sln
```
