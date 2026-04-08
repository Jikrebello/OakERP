# Findings

## Task
ar-invoice-backend

## Current State
- `ArInvoice` and `ArInvoiceLine` already exist with `RevenueAccount`, `ItemId`, `LocationId`, and `TaxRateId`.
- AR invoice posting already exists through `IPostingService` and loads the tracked AR invoice graph from `IArInvoiceRepository.GetTrackedForPostingAsync`.
- There is no `AccountsReceivable/Invoices` application namespace and no `ArInvoicesController`.
- `IArInvoiceRepository` does not yet expose an `ExistsDocNoAsync` helper for create-time uniqueness checks.

## Relevant Projects
- `OakERP.API`
- `OakERP.Application`
- `OakERP.Domain`
- `OakERP.Infrastructure`
- `OakERP.Tests.Unit`
- `OakERP.Tests.Integration`

## Dependency Observations
- The correct seam remains `API -> Application contracts/service/workflow -> Domain repository contracts -> Infrastructure repositories`.
- Create-time line-reference validation can use existing repository seams for customer, currency, GL account, item, location, and tax rate.

## Structural Problems
- Backend AR invoice capture is missing even though draft persistence and posting are already implemented.
- `ITaxRateRepository` exists only as a Domain contract today; no Infrastructure implementation is present.
- `IItemRepository` and `ILocationRepository` exist with Infrastructure implementations but are not registered in `AddRepositories()`.

## Literal / Model-Family Notes
- Repeated business-significant literals:
- No new domain-significant constants should be required for the create slice.
- Repeated domain-significant numbers:
  - Existing entity/config limits remain the source of truth: 40-char doc number, 512-char memo/description/ship-to, 20-char account number.
- Runtime-vs-persisted model-family conflicts:
  - None should be introduced. This slice must not redesign posting runtime models.
- Thin orchestrators getting too thick:
  - The controller and service should stay thin; precondition loading and entity construction belong in the workflow.
- Pure engines/calculators with side effects or lookups:
  - Posting engines/builders must remain untouched except for compatibility validation.

## Configuration / Environment Notes
- No new environment-specific configuration is needed.
- Swagger example parity is part of the current backend slice standard and should be kept in scope.

## Testing Notes
- Existing patterns are `[ApInvoiceApiTests]`, `[ArReceiptApiTests]`, and `[SwaggerDocumentTests]`.
- Existing AR invoice posting tests were blocked by fixed-key seed data colliding with globally seeded integration data.
- Compatibility validation required making `ArInvoicePostingTests.SeedInvoiceScenarioAsync` use idempotent/shared-safe seed setup and an explicit out-of-period invoice date for the missing-period negative case.
- The broader repo pass exposed the same shared-seed collision pattern in `ApInvoiceApiTests`, `ApPaymentApiTests`, `ArReceiptApiTests`, and `AuthApiTests`; those suites needed per-test unique tenant/document identifiers rather than shared fixture-level keys.
- `ApPaymentApiTests.SeedPostedInvoiceAsync` also generated an `InvoiceNo` longer than the persisted 40-character limit once document numbers were made unique; the helper needed a shorter deterministic invoice number.

## Rollback / Transaction Notes
- Migration rollback reviewed:
- Transactional failure leaves no writes:
- Not applicable unless a hard blocker forces schema work.
- Must be validated by integration tests for duplicate doc number and invalid reference-data failures.

## Open Questions
- None after the approved refinement.

## Deferred Smells / Risks
- No frontend/UI work.
- No bank transaction creation.
- No reversal or unposting.
- No posting transport redesign.
- No broader repository redesign.
- No broad test-harness redesign; only narrow seed-helper hardening and per-test unique identifiers were applied where required to validate the slice in the current shared integration environment.
- Posting-time restrictions such as base-currency-only posting, fiscal-period checks, revenue fallback resolution, costing, and GL/inventory effects remain deferred to the existing posting slice.

## Recommendation
Implement the missing AR invoice create slice using the existing aggregate shape and posting-compatible draft data, with only a narrow `ExistsDocNoAsync` repository addition and the smallest DI/repository wiring needed for item/location/tax validation.

