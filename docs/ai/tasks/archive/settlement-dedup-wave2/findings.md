# Findings

## Task
settlement-dedup-wave2

## Current State
- `ApPaymentService` and `ArReceiptService` each contain a private `LoadTrackedInvoicesAsync` and `ApplyAllocationsAsync`.
- The two implementations are structurally the same and differ mainly in repository call shape, ownership field (`VendorId` vs `CustomerId`), calculator family, allocation entity type, and failure text.
- Snapshot factories and command validators are already family-specific and should stay that way.

## Risks
- Over-generalizing the shared settlement logic would make the services harder to read than the current duplication.
- Moving failure-message construction into the shared layer would make behavior drift more likely.

## Intended Shape
- Shared generic helpers in `OakERP.Application.Settlements`.
- Small AP/AR adapter bundles near the family services.
- Existing service entrypoints remain responsible for validation, transaction scope, and result shaping.

## Outcome Notes
- The shared seam works cleanly as two focused helpers: one for invoice loading and one for allocation application.
- The adapter-bundle approach kept family-specific failure text and repository call shape out of the shared layer.
- `InternalsVisibleTo` was needed so the shared helper tests could validate the new internal seam directly without making the helper types public.
