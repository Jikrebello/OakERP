# Findings

## Task
ap-payment-posting

## Current State
- `IPostingService`, `PostCommand`, and `PostResult` already exist and are used for AP invoice, AR invoice, and AR receipt posting.
- `ApPayment` already contains `DocStatus`, `PaymentDate`, `PostingDate`, `Amount`, `BankAccountId`, and `Allocations`, which is enough for a no-schema AP payment posting slice.
- `ApPaymentCommandValidator` already enforces draft-capture rules for active vendor, active bank account, positive amount, unique doc number, base-currency bank account, and allocation limits.
- `ApPayment` has no persisted `CurrencyCode`; this slice should stay base-currency-only by validating the linked bank account currency against the configured base currency.
- `IBankTransactionRepository` and `BankTransaction` exist, but they are not wired into the posting runtime and current posting tests encode a zero-bank-transaction pattern.

## Dependency Observations
- The posting seam already belongs in Application and Infrastructure; no API controller or new transport contract is needed.
- A narrow `IApPaymentRepository.GetTrackedForPostingAsync` method remains inside repository ownership and avoids direct `DbContext` use in `PostingService`.
- Runtime AP payment posting models belong under `OakERP.Domain.Posting.Accounts_Payable`.

## Structural Risks
- AP payment allocations already affect AP invoice settlement state while the payment is still draft. This slice should keep that timing unchanged rather than widening into a settlement redesign.
- `PostingService` is exception-based for expected posting failures. This slice should stay on the established posting seam instead of widening into a result-contract redesign.
- Bank transaction creation is tempting because the entity and repository already exist, but DI and result plumbing are intentionally not part of the current posting slices.

## Implementation Notes
- AP payment posting should debit bank for the full payment amount and credit AP control for the full payment amount.
- Allocated and unapplied AP payments should produce the same GL shape in this slice.
- Source traceability should use a new centralized source type constant for AP payments.
- AP payment posting should write GL rows only, with zero inventory and zero bank transaction rows.

## Rollback / Transaction Notes
- No migration or rollback review is required unless implementation proves a hidden schema requirement.
- No schema change or migration was required for this slice, so migration rollback review was not applicable.
- Transactional failure behavior was validated through unit and integration tests covering over-allocation rejection, no-open-period rejection, non-base-currency rejection, double-post resistance, concurrent-post resistance, and unexpected-inventory-output rejection.

## Domain-Significant Additions
- Added `PostingSourceTypes.ApPayment`

## Deferred Areas
- bank transaction creation
- AP payment reversal or unposting
- FX, discount, and write-off posting
- dedicated unapplied-credit liability behavior
- redesign of draft-allocation settlement timing
