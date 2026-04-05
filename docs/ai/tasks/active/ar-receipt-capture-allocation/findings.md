# Findings

## Task
ar-receipt-capture-allocation

## Current State
- `ArReceipt` and `ArReceiptAllocation` already exist in Domain with EF configuration and persisted tables.
- `IArReceiptRepository`, `IArReceiptAllocationRepository`, `ICustomerRepository`, and `IBankAccountRepository` already exist, with matching infrastructure implementations.
- `IArInvoiceRepository` already exists, but only has a posting-specific tracked-load method; it does not yet expose a tracked-load path for allocation work.
- `OakERP.Application` currently contains posting contracts only plus `IUnitOfWork`; there is no AR receipt application service or DTO layer.
- `OakERP.API` currently exposes auth endpoints only; there is no finance CRUD/controller pattern yet.
- `ServiceCollectionExtensions.AddRepositories()` currently wires `IArInvoiceRepository` but not the receipt, customer, or bank-account repositories already present on disk.
- `ApplicationDbContext` maps `AROpenItemView`, but that view is not used anywhere in the current backend flow.

## Dependency Observations
- The clean place for the new use-case contract is `OakERP.Application`.
- The implementation belongs in `OakERP.Infrastructure`, consistent with the current `PostingService`.
- Controllers should depend on application contracts or DTO/result types, not on `ApplicationDbContext`.
- API should remain thin and should not own allocation/business rules.

## Structural Problems
- The backend has persisted receipt entities without a corresponding application/API slice.
- Receipt/customer/bank-account repositories exist but are not registered in DI.
- There is no existing business-failure DTO/result path for finance work, only auth-local examples.
- There is no existing receipt tracked-load repo method that includes allocations for precise math.

## Allocation / State Notes
- `ArInvoice` stores `DocTotal`, `DocStatus`, and `Allocations`, but not a persisted remaining balance.
- `public.v_ar_open_items` derives balance from `SUM(amount_applied)` only and includes both `posted` and `closed` invoices; it is not suitable as the authoritative allocation math source for this slice.
- `ArReceiptAllocation` already includes `DiscountGiven` and `WriteOffAmount`, but this slice can safely leave those null and out of request contracts.

## Currency / Posting Notes
- `ArReceipt` includes `CurrencyCode`, `AmountForeign`, and `ExchangeRate`.
- Existing AR invoice posting explicitly rejects non-base-currency invoices.
- There is no receipt-posting context/builder/engine yet; `IPostingEngine` only comments that as future work.

## Testing Notes
- Unit tests already use strict factory-based mocking for service slices.
- Integration tests already use the full API host and direct DB assertions for posting slices.
- The integration harness resets the database and reruns seeders before each test, so receipt tests can seed only the entities they require.

## Rollback / Transaction Notes
- No migration rollback review required yet because the planned slice should not need a schema change by default.
- Transactional failure behavior must be validated because create+allocate and allocate-only both write across multiple rows and update invoice state.
- Integration testing exposed an EF concurrency failure when allocating against an existing tracked draft receipt by mutating the tracked receipt collection directly.
- The safe narrow fix was to create new allocation rows through the existing `IArReceiptAllocationRepository` and build response math from explicit in-service allocation snapshots, avoiding any repository redesign or reliance on `v_ar_open_items`.

## Deferred Smells / Risks
- The receipt and AP payment models are structurally similar, but introducing a shared payment/allocation abstraction would broaden scope and is deferred.
- Foreign-currency receipt allocation semantics are intentionally deferred.
- Read/list/query endpoints are intentionally deferred.
