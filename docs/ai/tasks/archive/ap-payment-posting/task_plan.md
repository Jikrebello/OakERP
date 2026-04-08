# Task Plan

## Task Name
ap-payment-posting

## Goal
Implement the smallest safe backend-first AP payment posting slice through the existing `IPostingService.PostAsync` seam, limited to base-currency GL posting for bank and AP control only.

## Background
AP payment draft capture and allocation already exist through the API and service layer. This slice should add draft AP payment posting without widening into transport, bank transactions, reversal, FX, or schema work.

## Scope
- `OakERP.Domain`
- `OakERP.Infrastructure`
- `OakERP.Tests.Unit`
- `OakERP.Tests.Integration`
- `docs/ai/tasks/active/ap-payment-posting/`

## Out of Scope
- new API/controller/transport entrypoints
- AP payment capture or allocation redesign
- bank transaction creation
- unposting or reversal support
- schema changes or migrations
- FX, discount, write-off, or inventory behavior
- AP payment settlement-timing redesign

## Constraints
- Keep the app-facing seam unchanged: `IPostingService.PostAsync(new PostCommand(DocKind.ApPayment, ...))`
- Keep `PostingService` orchestration thin.
- Keep AP payment posting limited to GL rows plus payment status transition.
- Write zero inventory rows and zero bank transaction rows.
- Treat unapplied AP payments as `Dr Bank / Cr AP Control` for the full payment amount.
- Keep the current draft-allocation settlement timing unchanged.
- Use the posting command date operationally and through `ApPayment.PostingDate` plus `GlEntry.EntryDate`.

## Success Criteria
- [x] `IPostingService.PostAsync` supports `DocKind.ApPayment`
- [x] Draft AP payments can be posted transactionally to GL
- [x] Posting writes balanced bank and AP-control GL entries only
- [x] AP payments move from `Draft` to `Posted` and populate `PostingDate`
- [x] Double-post attempts do not create duplicate GL rows
- [x] Unit and integration tests cover success, rejection, concurrency, and rollback behavior
- [x] Task docs record findings, validation, constants, and deferrals

## Planned Steps
1. Create task docs and record the current AP payment/posting findings.
2. Add narrow AP payment posting runtime models, context builder, and repository load support.
3. Extend the existing posting rule provider, posting engine, and posting service for `DocKind.ApPayment`.
4. Add unit tests for AP payment posting orchestration and engine behavior.
5. Add integration tests for AP payment posting persistence and transaction boundaries.
6. Run validation in the required order and update `progress.md`.

## Validation Commands
```powershell
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter FullyQualifiedName~Posting
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ApPaymentPosting
dotnet build OakERP.API/OakERP.API.csproj
pwsh ./tools/validate-pr.ps1
```
