# Findings

## Task
ar-receipt-posting

## Current State
- `IPostingService`, `PostCommand`, and `PostResult` already exist and are currently used for AR invoice posting only.
- `PostingService` supports only `DocKind.ArInvoice` and throws for unsupported kinds.
- `IPostingEngine` and `PostingSourceTypes` are AR-invoice-only today.
- `ArReceipt` already contains `DocStatus`, `PostingDate`, `BankAccountId`, `Amount`, `CurrencyCode`, and `Allocations`, which is enough for a no-schema receipt posting slice.
- `BankAccount.GlAccountNo` already provides the bank-side GL account.
- `GlEntry` already supports source traceability through `SourceType`, `SourceId`, and `SourceNo`.
- `BankTransaction` and `IBankTransactionRepository` exist but are currently unused by posting and are outside this MVP.

## Dependency Observations
- The existing posting seam belongs in Application and Infrastructure; no new API endpoint is needed for this slice.
- A narrow `IArReceiptRepository.GetTrackedForPostingAsync` method remains within repository ownership and avoids direct `DbContext` use in `PostingService`.
- A runtime receipt posting context belongs under `OakERP.Domain.Posting.Accounts_Receivable`.

## Structural Risks
- `PostingService` currently uses AR-invoice-specific constructor dependencies and validation helpers; extending it must stay additive rather than turn into a broader posting redesign.
- Receipt capture/allocation currently computes receipt allocation math separately from posting; this slice should validate persisted allocation sums but must not redesign that coupling.
- `GlPostingSettings` has no dedicated unapplied-cash liability account, so unapplied cash must stay within AR control in this MVP.
- The existing posting seam is exception-based rather than DTO-result-based. Keeping `IPostingService.PostAsync(PostCommand)` unchanged means this slice stays on the established posting contract instead of widening into a posting contract redesign.

## Implementation Notes
- The slice added a narrow tracked receipt-posting load on `IArReceiptRepository` and did not require any schema change.
- Receipt posting uses a dedicated runtime context and context builder under `OakERP.Domain.Posting.Accounts_Receivable`.
- `PostingSourceTypes` now includes a receipt source constant, and the existing code-backed runtime rule provider now returns both AR invoice and AR receipt rules.
- Receipt posting writes GL entries only and rejects any posting output that contains inventory rows.
- Unapplied cash posts as `Dr Bank / Cr AR Control` for the full receipt amount; allocation state remains visible through persisted receipt allocations rather than separate GL accounts.

## Rollback / Transaction Notes
- No migration or rollback review was required because this slice did not introduce a schema change.
- Transactional failure behavior was validated through unit and integration tests covering save failure, over-allocation rejection, no-open-period rejection, non-base-currency rejection, double-post resistance, and concurrent-post resistance.

## Domain-Significant Additions
- Added `PostingSourceTypes.ArReceipt`.

## Deferred Areas
- bank transaction creation
- unposting/reversal
- FX receipt posting
- discount/write-off posting
- dedicated unapplied-cash liability design
- posting transport endpoints
