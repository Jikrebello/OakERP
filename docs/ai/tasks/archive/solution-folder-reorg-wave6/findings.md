# Findings

## Current State

- `OakERP.Auth` now groups source by responsibility under `Services`, `Identity`, `Jwt`, and `Extensions`.
- `OakERP.Application` is now organized by operation slice: AP invoices, AP payments, AR receipts, posting contracts and operations, and settlement helpers all sit in focused subfolders with matching namespaces.
- `OakERP.Common`, `OakERP.Client`, `OakERP.Domain`, and `OakERP.Infrastructure` no longer use apostrophe- or underscore-based folder names for the targeted slices.
- `OakERP.Tests.Integration/ApiRoutes.cs` now lives under `TestSetup` with a matching namespace.
- `OakERP.API`, `OakERP.Shared`, `OakERP.UI`, and `OakERP.Web` only required reference fallout and did not need structural churn.

## Risks

- Namespace-only refactors can change logger category names and other type-name-derived behavior even when runtime logic stays the same.
- Large folder moves can leave behind compatibility shims or empty placeholder folders unless the final pass removes them explicitly.

## Observed Fallout

- `RuntimeSupportTests` originally expected the pre-move `AuthService` logger category and had to be updated to use `typeof(AuthService).FullName` instead of a hard-coded namespace string.
- The architecture test for `ApplicationUser` ownership still expected the earlier `OakERP.Auth.ApplicationUser` namespace and had to be updated to the final `OakERP.Auth.Identity.ApplicationUser` location.
