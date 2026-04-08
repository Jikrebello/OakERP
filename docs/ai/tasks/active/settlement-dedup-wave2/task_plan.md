# Task Plan

## Task
settlement-dedup-wave2

## Summary
Deduplicate the AP payment and AR receipt settlement workflow now living in `OakERP.Application` by extracting shared invoice-loading and allocation-application helpers, while preserving existing service interfaces, behavior, and response payloads.

## Constraints
- No HTTP, DTO, schema, auth, posting, or host wiring changes.
- Keep `IApPaymentService` and `IArReceiptService` unchanged.
- Avoid abstract base services; use focused shared helpers and family-specific adapter bundles.

## Ordered Steps
1. Create wave-2 task records and capture the duplicated settlement findings.
2. Add an internal `OakERP.Application.Settlements` slice with shared loader and applicator helpers.
3. Add AP and AR adapter bundles that supply family-specific repository calls, calculator hooks, and failure messages.
4. Refactor `ApPaymentService` and `ArReceiptService` to use the shared helpers and remove the duplicated private methods.
5. Add targeted unit tests for the shared helpers and keep the current AP/AR service tests green.
6. Run targeted builds/tests first, then broader solution validation.

## Validation Plan
- `dotnet build OakERP.Application/OakERP.Application.csproj`
- `dotnet build OakERP.Tests.Unit/OakERP.Tests.Unit.csproj`
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj`
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj`
- `dotnet build OakERP.sln`
