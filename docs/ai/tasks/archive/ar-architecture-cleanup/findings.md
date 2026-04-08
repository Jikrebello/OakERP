# Findings

## Task
ar-architecture-cleanup

## Current State
Before cleanup, AR receipt capture/allocation and AR receipt posting were already working and tested. The main issues were ownership and duplication rather than broken behavior.

## Relevant Projects
- `OakERP.Domain`
- `OakERP.Infrastructure`
- `OakERP.Tests.Unit`

## Dependency Observations
- AR contracts remained in Application and implementations remained in Infrastructure.
- Repository additions from prior slices were still narrow and aggregate-local.
- Shared AR posting behavior had outgrown invoice-specific class names.

## Structural Problems
- `ArReceiptService` owned request validation, normalization, snapshot shaping, and orchestration in one class.
- Receipt settlement math was duplicated in service and posting code.
- Receipt posting context carried fields not used by the runtime engine.
- Shared AR posting types were named as invoice-only types.

## Literal / Model-Family Notes
- Repeated business-significant literals:
  - none introduced in this cleanup slice
- Repeated domain-significant numbers:
  - none introduced in this cleanup slice
- Runtime-vs-persisted model-family conflicts:
  - `OakERP.Domain.Entities.Posting.PostingRule*` and `OakERP.Domain.Posting.PostingRule*` still coexist and remain deferred
- Thin orchestrators getting too thick:
  - `ArReceiptService` was the main pressure point and was reduced
- Pure engines/calculators with side effects or lookups:
  - posting engine remained pure

## Configuration / Environment Notes
- No new configuration values were added.
- No hardcoded environment assumptions were introduced.

## Testing Notes
- Added direct unit coverage for:
  - `ArSettlementCalculator`
  - `ArReceiptCommandValidator`
  - `ArReceiptSnapshotFactory`
- Existing AR service/posting unit and integration tests still pass.

## Rollback / Transaction Notes
- Migration rollback reviewed: not applicable, no schema work
- Transactional failure leaves no writes:
  - preserved from existing receipt capture/allocation and posting flows; validated through existing posting and receipt service tests plus integration suites

## Open Questions
- None required for this slice.

## Deferred Smells / Risks
- `IPostingService` still returns exceptions for expected business failures.
- `OakERP.Domain` still directly references Identity EF Core and is not framework-light enough.
- Runtime and persisted posting-rule families still have overlapping names.

## Recommendation
Keep the next cleanup slice separate and focused on one of:
- posting result-path standardization
- Domain dependency cleanup
- posting-rule family naming clarification
