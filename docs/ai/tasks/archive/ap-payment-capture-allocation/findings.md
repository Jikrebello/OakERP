# Findings

## Task
ap-payment-capture-allocation

## Current State
- `ApPayment` and `ApPaymentAllocation` already exist in Domain and are mapped in EF Core.
- `IApPaymentRepository` and `IApPaymentAllocationRepository` already exist with basic CRUD-style methods.
- `ApInvoice` already has `Allocations` and `DocStatus`, and AP invoice posting is already implemented.
- There was no AP payment Application namespace surface, no AP payment controller, no AP payment service, and no AP payment tests before this task.
- `public.v_ap_open_items` exists as a reporting view, but it is not an appropriate write-path source of truth for allocation math.

## Relevant Projects
- `OakERP.Application`
- `OakERP.Domain`
- `OakERP.Infrastructure`
- `OakERP.API`
- `OakERP.Tests.Unit`
- `OakERP.Tests.Integration`

## Dependency Observations
- The correct seam for this slice is `API -> Application contract -> Infrastructure service -> Domain repositories/entities/helper`.
- Narrow repo helpers are sufficient: tracked payment load with allocations/bank account, doc-number existence, and tracked AP invoice load with allocations.

## Structural Problems
- AP payment persistence existed without a backend vertical slice for capture/allocation.
- AP had no settlement helper equivalent to AR’s receipt/invoice settlement calculator.

## Literal / Model-Family Notes
- No new runtime posting models are required in this slice.
- Existing model-family separation between persisted AP entities and posting runtime models remains intact.

## Configuration / Environment Notes
- The slice depends on existing posting settings to read the base currency code.
- No new environment-specific configuration or secrets should be introduced.

## Testing Notes
- This task requires unit coverage for AP payment orchestration/settlement math and integration coverage for API wiring, persistence, and transaction behavior.

## Rollback / Transaction Notes
- Migration rollback reviewed:
  - Not applicable. No schema work was added.
- Transactional failure leaves no writes:
  - Validated through integration tests that reject mismatched vendors, over-allocation, and non-base-currency paths without persisting payments or allocations.

## Deferred Smells / Risks
- AP payment posting remains a separate later slice.
- `DiscountTaken`, `WriteOffAmount`, FX, bank transactions, and payment reversal flows remain deferred.
- `ApPayment` still lacks a persisted currency field; this slice intentionally does not solve that broader model concern.

## Recommendation
The next safe AP backend slice after this one is AP payment posting, extending the existing posting seam without widening this draft capture/allocation slice into bank or reversal behavior.
