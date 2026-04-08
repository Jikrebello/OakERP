# Findings

## Task
posting-remediation-slice

## Current State
AR invoice posting slices 1A and 1B are implemented and validated. The narrow posting path is working, but the audit found a small set of concrete maintainability issues that should be corrected before more posting work is layered on.

## Relevant Projects
- `OakERP.Domain`
- `OakERP.Infrastructure`
- `OakERP.Tests.Unit`
- `OakERP.Tests.Integration`

## Dependency Observations
- `OakERP.Infrastructure.Posting.PostingService` stays on repository and unit-of-work abstractions.
- `OakERP.Infrastructure.Posting.AccountsReceivable.ArInvoicePostingEngine` is still pure with respect to persistence and lookups.
- `OakERP.Domain.Posting.PostingRule` currently imports `OakERP.Domain.Entities.Posting`, which leaks the persisted posting model family into the runtime posting path.

## Structural Problems
- `AddArInvoiceLineLocation` has a broken `Down()` path that drops unrelated `inventory_ledgers` objects.
- Domain-significant posting literals are repeated as raw strings across runtime code and tests.
- `PostingService.ValidatePostingResult(...)` does not yet validate inventory math consistency or traceability invariants.
- One unit test claims revenue fallback coverage but only tests pre-resolved accounts.
- `OakERP.Domain.Posting.PostingRule` currently imports `OakERP.Domain.Entities.Posting`, which leaks the persisted posting model family into the runtime posting path.

## Configuration / Environment Notes
- `gl.posting` is currently a raw string key in the GL settings provider and seeded integration data.
- `FiscalPeriod.Status` still uses string values such as `"open"`.

## Testing Notes
- Posting tests are already present and focused.
- Migration rollback is not covered by the standard test suite and needs an explicit validation step.
- The misleading revenue fallback test belongs at the builder layer, not the engine layer.

## Open Questions
- None currently blocking this narrow remediation slice.

## Recommendation
Implement a small remediation slice that fixes the bad migration rollback, removes the runtime posting model leak, centralizes the minimum posting literals, strengthens posting validation, replaces the misleading fallback test, and updates the repo guidance files so future Codex work follows the same standards.
