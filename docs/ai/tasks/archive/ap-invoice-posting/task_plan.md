# Task Plan

## Task Name
ap-invoice-posting

## Goal
Implement the smallest safe backend-first AP invoice posting slice through the existing `IPostingService.PostAsync` seam, limited to base-currency GL posting for AP control, expense lines, and header tax only.

## Background
AP invoice draft capture already exists through the API and service layer. This slice should add draft AP invoice posting without widening into transport, AP payment, bank behavior, inventory, reversal, or schema work.

## Scope
- `OakERP.Domain`
- `OakERP.Infrastructure`
- `OakERP.Tests.Unit`
- `OakERP.Tests.Integration`
- `docs/ai/tasks/active/ap-invoice-posting/`

## Out of Scope
- new API/controller/transport entrypoints
- AP invoice capture redesign
- AP payment capture or allocation
- bank transaction creation
- unposting or reversal support
- schema changes or migrations
- FX, discount, write-off, or inventory behavior

## Constraints
- Keep the app-facing seam unchanged: `IPostingService.PostAsync(new PostCommand(DocKind.ApInvoice, ...))`
- Keep `PostingService` orchestration thin.
- Keep AP posting limited to GL rows plus invoice status transition.
- Write zero inventory rows and zero bank/cash rows.
- Post header `TaxTotal` to `DefaultTaxInputAccountNo` when `TaxTotal > 0`.
- Use the posting command date operationally and through GL entry dates only.
- Extend the existing posting runtime in place and document AR-biased naming debt instead of refactoring it.

## Success Criteria
- [x] `IPostingService.PostAsync` supports `DocKind.ApInvoice`
- [x] Draft AP invoices can be posted transactionally to GL
- [x] Posting writes balanced AP-control, expense, and optional tax-input GL entries only
- [x] AP invoices move from `Draft` to `Posted`
- [x] Double-post attempts do not create duplicate GL rows
- [x] Unit and integration tests cover success, rejection, concurrency, and rollback behavior
- [x] Task docs record findings, validation, constants, and deferrals

## Planned Steps
1. Create task docs and record the current AP/posting findings.
2. Add narrow AP posting runtime models, context builder, and repository load support.
3. Extend the existing posting rule provider, posting engine, and posting service for `DocKind.ApInvoice`.
4. Add unit tests for AP posting orchestration, context building, and engine behavior.
5. Add integration tests for AP invoice posting persistence and transaction boundaries.
6. Run validation in the required order and update `progress.md`.

## Validation Commands
```powershell
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter FullyQualifiedName~Posting
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ApInvoicePosting
dotnet build OakERP.API/OakERP.API.csproj
pwsh ./tools/validate-pr.ps1
```
