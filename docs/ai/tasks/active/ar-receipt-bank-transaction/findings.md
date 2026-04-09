# Findings

## Task
ar-receipt-bank-transaction

## Current State
- AR receipt capture/allocation already validates and stores `BankAccountId` through `ArReceiptCreateWorkflow`.
- AR receipt posting already runs through `ArReceiptPostingOperation` inside `PostingTransactionExecutor`.
- `ArReceiptPostingOperation` already owns the tracked receipt, resolved posting date, bank-account validation, and the posting transaction boundary.
- `BankTransaction`, `IBankTransactionRepository`, `BankTransactionRepository`, and `BankTransactionConfiguration` already exist.
- `BankTransactionConfiguration` already provides a filtered unique index on `(SourceType, SourceId)` and the `dr_account_no <> cr_account_no` check constraint.
- `IBankTransactionRepository` is not registered in infrastructure DI today.
- Current AR receipt posting tests explicitly assert zero bank transactions.

## Relevant Projects
- `OakERP.Application`
- `OakERP.Infrastructure`
- `OakERP.Tests.Unit`
- `OakERP.Tests.Integration`

## Dependency Observations
- The cleanest insertion point is `ArReceiptPostingOperation`, not `PostingEngineResult` or `PostingResultProcessor`.
- The shared posting runtime/result contracts should stay GL/inventory focused; bank transaction persistence is receipt-specific orchestration.
- Wiring the repository directly into `PostingService` and then only into `ArReceiptPostingOperation` keeps the change local without expanding the shared persistence dependency bag.

## Structural Problems
- AR receipt posting currently leaves the cash-side persistence artifact missing even though the bank transaction model already exists.
- The repository exists but is unusable from runtime DI until it is registered.

## Literal / Model-Family Notes
- Repeated business-significant literals:
- `PostingSourceTypes.ArReceipt` already exists and should be reused for the bank transaction source traceability.
- Repeated domain-significant numbers:
- None introduced by this slice.
- Runtime-vs-persisted model-family conflicts:
- Avoid adding `BankTransaction` to `PostingEngineResult`; keep persisted bank rows out of the pure posting runtime model family.
- Thin orchestrators getting too thick:
- `ArReceiptPostingOperation` can own one additional persistence step without becoming a redesign; pushing this into broader shared posting layers would be a worse fit.
- Pure engines/calculators with side effects or lookups:
- `PostingEngine.PostArReceipt` must remain pure and unchanged.

## Configuration / Environment Notes
- No configuration or environment changes are needed for this slice.

## Testing Notes
- Unit tests should prove bank transaction field mapping and rollback when bank transaction persistence throws before commit.
- Integration tests should prove one-row creation on success, no duplicates on second post, and full rollback when save fails because the bank transaction violates the existing `dr_account_no <> cr_account_no` check constraint.
- API integration tests only need to confirm the existing post endpoint contract is unchanged while the bank transaction side effect is present.

## Rollback / Transaction Notes
- Migration rollback reviewed: not applicable, no schema change is planned.
- Transactional failure leaves no writes: validate with an integration scenario that forces `dr_account_no == cr_account_no` on the bank transaction save path so the database rejects the transaction and no GL or bank rows commit.

## Open Questions
- None after repo inspection.

## Deferred Smells / Risks
- AP payment bank transaction creation remains the immediate follow-on slice.
- Reconciliation, statement import/matching, reversal, and FX behavior remain intentionally deferred.

## Recommendation
Register `IBankTransactionRepository`, add one bank transaction persistence step inside `ArReceiptPostingOperation`, and extend the posting tests to prove success-side persistence plus rollback-on-save-failure without changing API contracts or widening the posting runtime model.

