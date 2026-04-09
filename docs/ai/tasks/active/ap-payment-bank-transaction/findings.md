# Findings

## Task
ap-payment-bank-transaction

## Current State
AP payment create/allocation, posting, and posting transport already existed. The shared `BankTransaction` model, repository, EF configuration, and DI registration already existed from the AR receipt bank transaction slice. `ApPaymentPostingOperation` was still GL-only, and AP payment runtime rules/tests still hard-coded debit-bank/credit-AP polarity.

## Relevant Projects
- `OakERP.Application`
- `OakERP.Infrastructure`
- `OakERP.Tests.Unit`
- `OakERP.Tests.Integration`

## Dependency Observations
- `PostingService` already injects `IBankTransactionRepository` and passes it to `ArReceiptPostingOperation`, so AP payment could mirror that without adding a new abstraction.
- `ApPaymentPostingOperation` already owns the tracked payment, resolved posting date, bank account, and transaction boundary, which makes it the correct insertion point for persisted bank transaction creation.

## Structural Problems
- `PostingRuleProvider`, `PostingEngine`, and AP payment posting tests encoded debit-bank/credit-AP, which contradicted outbound-cash bank transaction semantics.
- AP payment posting left the cash-side persisted artifact missing even though the shared bank transaction model was already available.

## Literal / Model-Family Notes
- Repeated business-significant literals:
  - AP payment polarity expectations appeared in runtime rules, the posting engine, and AP payment tests and needed to change together.
- Repeated domain-significant numbers:
  - Existing test accounts remained `2000` for AP control and `1000` for bank.
- Runtime-vs-persisted model-family conflicts:
  - Keep GL output generation in the posting engine and bank transaction persistence in the posting operation; do not push bank rows into posting engine contracts.
- Thin orchestrators getting too thick:
  - `PostingService` remained a dependency pass-through only.
- Pure engines/calculators with side effects or lookups:
  - `PostingEngine` remained pure; persistence stayed in `ApPaymentPostingOperation`.

## Configuration / Environment Notes
- No environment or configuration changes were required.

## Testing Notes
- Existing AP payment unit and integration tests asserted the old polarity and zero bank transactions, so they had to be updated rather than worked around.
- AR receipt bank transaction tests provided the rollback and side-effect assertion pattern to mirror.

## Rollback / Transaction Notes
- Migration rollback reviewed:
  - No migration involved.
- Transactional failure leaves no writes:
  - Validated by forcing `DrAccountNo == CrAccountNo` on the bank transaction save path via a bank account GL account of `2000`, which triggers the existing `ck_banktxn_dr_neq_cr` constraint and leaves no committed GL rows, no bank transaction row, and the payment still draft.

## Open Questions
- None for this slice after approval. The polarity correction scope and insertion point were explicitly agreed before implementation.

## Deferred Smells / Risks
- Bank reconciliation, statement import, reversal/unposting, and FX behavior remain intentionally out of scope.
- AP payment settlement redesign and any generic cash-management abstraction remain deferred.

## Recommendation
Correct AP payment polarity in the runtime rule provider and posting engine, then add one outbound bank transaction inside `ApPaymentPostingOperation` and update all AP payment test fixtures/assertions to pin the corrected semantics end to end.
