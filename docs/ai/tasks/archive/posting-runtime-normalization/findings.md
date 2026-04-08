# Findings

## Task
posting-runtime-normalization

## Current State
- `IPostingEngine` and `IPostingRuleProvider` already live in `OakERP.Domain.Posting` and are generic runtime contracts.
- `PostingService` already lives in `OakERP.Infrastructure.Posting` and orchestrates `ApInvoice`, `ArInvoice`, and `ArReceipt` posting through the shared runtime contracts.
- The shared concrete runtime has now been normalized to `PostingEngine` and `PostingRuleProvider` under `OakERP.Infrastructure.Posting`.
- The DI registration in `ServiceCollectionExtensions.AddPostingServices()` now resolves `IPostingEngine` and `IPostingRuleProvider` through those shared names.
- AP-specific and AR-specific posting context builders already have correct document-specific placement and do not need re-homing in this cleanup slice.

## Dependency Observations
- The shared concrete runtime implementation belongs in `OakERP.Infrastructure.Posting`, not under `AccountsReceivable`.
- Document-specific context builders remain correctly placed under `AccountsPayable` and `AccountsReceivable`.
- The cleanup can stay entirely inside Infrastructure and unit-test imports, with no application contract or API surface changes.
- Actual import churn stayed narrow: DI registration plus the direct posting-engine unit tests.

## Structural Risks
- The main risk was import/namespace churn from renaming concrete types that were referenced directly by DI and unit tests; that remained bounded to those callers.
- `PostingService.ValidatePostingResult` contained AR-invoice-specific inventory error text inside a shared helper and was normalized without changing validation behavior.
- This slice should not drift into broader runtime redesign such as per-document handlers or new abstraction layers.

## Rollback / Transaction Notes
- No schema or migration work is required, so migration rollback review is not applicable.
- No transactional behavior was changed; existing posting integration tests remained the proof that runtime behavior stayed unchanged.

## Domain-Significant Additions
- None expected.

## Deferred Areas
- redesign of `IPostingEngine` or `IPostingRuleProvider`
- `PostingService` decomposition
- new posting document types
- result-path redesign for expected posting failures
- runtime-vs-persisted `PostingRule` family clarification
