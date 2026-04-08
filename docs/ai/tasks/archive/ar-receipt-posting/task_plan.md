# Task Plan

## Task Name
ar-receipt-posting

## Goal
Implement the smallest safe backend-first AR receipt posting slice through the existing `IPostingService.PostAsync` seam, limited to base-currency GL posting for bank and AR control only.

## Background
AR invoice posting already exists. AR receipt capture and allocation now exist, but receipts remain draft-only. This slice should add draft receipt posting without widening into transport, bank transaction creation, FX, unposting, or schema work.

## Scope
- `OakERP.Domain`
- `OakERP.Infrastructure`
- `OakERP.Tests.Unit`
- `OakERP.Tests.Integration`
- `docs/ai/tasks/active/ar-receipt-posting/`

## Out of Scope
- new API/controller/transport entrypoints
- receipt capture/allocation redesign
- bank transaction creation
- unposting or reversal support
- schema changes or migrations
- FX, discount, or write-off posting
- dedicated unapplied-cash liability behavior

## Constraints
- Keep the app-facing seam unchanged: `IPostingService.PostAsync(new PostCommand(DocKind.ArReceipt, ...))`
- Keep `PostingService` orchestration thin.
- Keep posting output to GL rows only, with zero inventory rows.
- Treat unapplied cash as `Dr Bank / Cr AR Control` for the full receipt amount.
- Do not redesign the existing allocation/invoice-settlement coupling.
- Keep repository additions narrow and entity-local.

## Success Criteria
- [x] `IPostingService.PostAsync` supports `DocKind.ArReceipt`
- [x] Draft AR receipts can be posted transactionally to GL
- [x] Posting writes balanced bank/AR-control GL entries only
- [x] Receipts move from `Draft` to `Posted` with `PostingDate`
- [x] Allocated and unapplied receipts both post successfully using the same GL pattern
- [x] Double-post attempts do not create duplicate GL rows
- [x] Unit and integration tests cover success, rejection, concurrency, and rollback behavior
- [x] Task docs record findings, validation, and deferrals

## Planned Steps
1. Create task docs and record current-state receipt/posting findings.
2. Add narrow receipt-posting repository load support and a runtime receipt posting context.
3. Extend the existing posting engine/rule provider and posting service for `DocKind.ArReceipt`.
4. Add unit tests for receipt posting orchestration and engine behavior.
5. Add integration tests for receipt posting persistence and transaction boundaries.
6. Run validation in the required order and update `progress.md`.

## Validation Commands
```powershell
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter FullyQualifiedName~Posting
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ArReceiptPosting
dotnet build OakERP.sln
pwsh ./tools/validate-pr.ps1
```
