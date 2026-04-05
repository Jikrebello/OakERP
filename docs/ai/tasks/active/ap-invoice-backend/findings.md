# Findings

## Task
ap-invoice-backend

## Current State
- `ApInvoice`, `ApInvoiceLine`, `Vendor`, `ApPayment`, and `ApPaymentAllocation` already exist in Domain and are mapped in EF Core.
- `IApInvoiceRepository`, `IApInvoiceLineRepository`, and `IVendorRepository` already exist with basic CRUD-style methods.
- AP invoice DB constraints already enforce unique `DocNo`, unique `(VendorId, InvoiceNo)`, nonnegative totals, and `DueDate >= InvoiceDate`.
- There was no Application AP namespace, no AP invoice API controller, no AP service, and no AP capture tests before this task.
- `ICurrencyRepository` and `CurrencyRepository` already existed but were not registered in DI.

## Relevant Projects
- `OakERP.Application`
- `OakERP.Domain`
- `OakERP.Infrastructure`
- `OakERP.API`
- `OakERP.Tests.Unit`
- `OakERP.Tests.Integration`

## Dependency Observations
- The correct seam for this slice is `API -> Application contract -> Infrastructure service -> Domain repositories/entities`.
- Repository additions can stay entity-local by adding only AP invoice uniqueness helpers instead of introducing a broader AP query layer.

## Structural Problems
- AP persistence existed without an application/service/API seam, so the backend vertical slice was incomplete.
- DI registration had a gap for AP and common currency repositories needed by a safe AP capture flow.

## Literal / Model-Family Notes
- Repeated business-significant literals:
- No new domain-significant constants or enums were introduced in this slice.
- Repeated domain-significant numbers:
  - Existing entity/config limits remain the source of truth: `DocNo`/`InvoiceNo` length 40, memo/description length 512, account number length 20.
- Runtime-vs-persisted model-family conflicts:
  - None introduced. This slice does not touch posting runtime models.
- Thin orchestrators getting too thick:
  - Business validation is kept in `ApInvoiceCommandValidator`; `ApInvoiceService` remains orchestration-focused.
- Pure engines/calculators with side effects or lookups:
  - None added.

## Configuration / Environment Notes
- No new environment-specific configuration or secrets were introduced.
- The slice relies on existing seeded/authenticated integration-test setup.

## Testing Notes
- Unit tests were added for AP validator failures and AP service orchestration.
- Integration tests were added for the API route, transactional failure behavior, and persisted draft invoice state.

## Rollback / Transaction Notes
- Migration rollback reviewed:
  - Not applicable. No schema work was added.
- Transactional failure leaves no writes:
  - Validated through integration tests that reject duplicate vendor invoice numbers, item-based lines, and inactive accounts without persisting invoices or lines.

## Open Questions
- None for this slice.

## Deferred Smells / Risks
- AP invoice posting is still a separate later slice.
- AP payment capture/allocation/posting remains deferred.
- Item-based AP invoice behavior and landed-cost/inventory receipt behavior remain explicitly deferred.
- Tax-rate support remains explicitly deferred and is rejected in this slice instead of partially implemented.

## Recommendation
The next safe AP backend slice after this one is AP invoice posting, using the existing posting seam rather than widening this draft-capture slice further.

