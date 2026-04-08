# Findings

## Task
ap-invoice-posting

## Current State
- `IPostingService`, `PostCommand`, and `PostResult` already exist and are used for AR invoice and AR receipt posting.
- `PostingService` now supports `DocKind.ApInvoice` alongside the existing AR document kinds.
- `ApInvoice` already contains `DocStatus`, `CurrencyCode`, `TaxTotal`, `DocTotal`, and `Lines`, which is enough for a no-schema AP posting slice.
- `ApInvoice` does not have a persisted `PostingDate`; this slice should use the command posting date operationally and through `GlEntry.EntryDate`.
- `ApInvoiceCommandValidator` rejects `ItemId` and `TaxRateId` lines, but still allows non-zero header `TaxTotal`.
- `GlPostingSettings` already contains `ApControlAccountNo` and `DefaultTaxInputAccountNo`.

## Dependency Observations
- The posting seam already belongs in Application and Infrastructure; no API controller or new transport contract is needed.
- A narrow `IApInvoiceRepository.GetTrackedForPostingAsync` method remains inside repository ownership and avoids direct `DbContext` use in `PostingService`.
- Runtime AP posting models belong under `OakERP.Domain.Posting.AccountsPayable`.

## Structural Risks
- The shared runtime implementation classes are still AR-named (`ArPostingEngine`, `ArPostingRuleProvider`). This slice should extend them in place and record the naming debt rather than widening into a posting-runtime refactor.
- `PostingService.ValidatePostingResult` currently contains AR-specific text inside generic validation logic. AP posting should keep the validation helper generic enough for the new branch without broad redesign.
- AP capture already allows header `TaxTotal`, so AP posting must handle header tax rather than deferring it completely.
- The existing posting seam is exception-based rather than DTO-result-based. Keeping `IPostingService.PostAsync(PostCommand)` unchanged means this slice stays on the established posting contract instead of widening into a contract redesign.

## Implementation Notes
- AP invoice posting should credit AP control for the full document total, debit each expense line account for its line total, and debit tax input for header tax when present.
- AP invoice posting should write GL rows only, with zero inventory and zero bank/cash effects.
- Source traceability should use a new centralized source type constant for AP invoices.
- The AP runtime context lives under `OakERP.Domain.Posting.AccountsPayable`, while the shared runtime engine and rule provider remain in their existing AR-named infrastructure files as intentional deferred naming debt.

## Rollback / Transaction Notes
- No migration or rollback review is required unless implementation proves a hidden schema requirement.
- No schema change or migration was required for this slice, so migration rollback review was not applicable.
- Transactional failure behavior was validated through unit and integration tests covering total mismatch rejection, unexpected inventory output rejection, no-open-period rejection, non-base-currency rejection, persisted item/tax-rate line rejection, double-post resistance, and concurrent-post resistance.

## Domain-Significant Additions
- Added `PostingSourceTypes.ApInvoice`

## Deferred Areas
- AP payment capture and allocation
- bank transaction creation
- bank/cash GL behavior
- inventory and landed-cost behavior
- unposting/reversal
- FX, discount, or write-off behavior
- persisted AP invoice posting-date behavior
