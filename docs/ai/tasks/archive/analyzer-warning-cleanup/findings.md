# Findings

## Task
analyzer-warning-cleanup

## Current State
The reported diagnostics were concentrated in three patterns:
- dead or commented-out code in Auth
- pure validator/snapshot helper classes being instantiated and injected
- orchestration methods carrying too many collaborators or too much inline validation logic

## Relevant Projects
- `OakERP.Auth`
- `OakERP.Common`
- `OakERP.Domain`
- `OakERP.Infrastructure`
- `OakERP.Tests.Unit`

## Dependency Observations
- `ApInvoiceService` was receiving validator/snapshot helpers that were only used through static calls.
- `ApPaymentService` and `ArReceiptService` had the same smell plus enough remaining dependencies to justify a small dependency bundle for GL settings + unit of work.
- `PostingService` had 16 constructor parameters, but those dependencies naturally grouped into source repositories, persistence collaborators, runtime services, and posting context builders.

## Structural Problems
- Static helpers registered in DI obscured ownership and inflated constructor signatures.
- `ApInvoiceCommandValidator.ValidateRequest(...)` and `PostingService.ValidatePostingResult(...)` were doing too much inline work for a single method.
- Repeated string literal `"Posting rule is required."` was scattered across posting engine entry points.

## Literal / Model-Family Notes
- Repeated business-significant literals:
  - `"Posting rule is required."` in `PostingEngine`
- Repeated domain-significant numbers:
  - none introduced by this task
- Runtime-vs-persisted model-family conflicts:
  - none introduced or extended
- Thin orchestrators getting too thick:
  - `PostingService.ValidatePostingResult(...)` before refactor
- Pure engines/calculators with side effects or lookups:
  - none introduced

## Configuration / Environment Notes
- No configuration changes were needed.
- Validation showed no new machine-specific assumptions.

## Testing Notes
- Existing unit tests were enough because the change was structural and behavior-preserving.
- Unit test factories needed constructor updates after helper/static cleanup and posting dependency bundling.

## Rollback / Transaction Notes
- Migration rollback reviewed: not applicable; no migration behavior changed
- Transactional failure leaves no writes: existing rollback paths were preserved, but no new transactional behavior was introduced

## Open Questions
- Whether the repo wants a separate pass for analyzer suggestions inside generated migration files.

## Deferred Smells / Risks
- `dotnet format analyzers ... --severity info` still reports many `CA1861` suggestions inside generated migration `20251201170038_Init.cs`; this task intentionally left generated migration churn out of scope.

## Recommendation
Use the same workflow for the next warning batch: fix the specific diagnostics in the touched code, then run targeted analyzer verification so live-IDE noise does not linger unnoticed.

