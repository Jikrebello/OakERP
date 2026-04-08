# Findings

## Current State

- `OakERP.Application/Posting/PostingService.cs` still owns four full posting workflows plus transaction handling, validation, and persistence.
- The AP payment, AP invoice, AR invoice, and AR receipt flows all repeat the same transaction and concurrency envelope.
- Posting-result validation and GL/inventory row persistence are shared concerns but currently live as private methods on `PostingService`.
- Existing posting tests already cover successful posting, validation failures, persistence failures, and unsupported document kinds.

## Risks

- Posting is a core accounting path, so behavioral drift must be treated as high risk.
- Error messages are asserted in tests, so shared helpers must preserve the current wording.
- Over-abstracting the handlers would make the posting slice harder to reason about than the current duplication.

## Desired Shape

- `PostingService` dispatches by `DocKind`.
- Internal document-family operations own family-specific rules and context construction.
- Shared internal helpers own:
  - transaction begin/commit/rollback plus concurrency translation
  - posting-result validation
  - GL/inventory row persistence
  - shared fiscal-period/rule/settings lookup and common guards

## Deferred

- Auth namespace normalization remains a later wave.
- No attempt will be made here to redesign posting engines, posting rules, or persisted posting schema.
