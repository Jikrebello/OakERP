# Settlement And Auth Cleanup Wave 5

## Scope

Reduce settlement generic/spec complexity, simplify `ApInvoiceService`, and refactor `AuthService` into smaller workflow methods without changing behavior.

## Constraints

- Preserve AP/AR settlement behavior and response payloads.
- Preserve auth registration/login behavior, audit logging semantics, and HTTP-facing DTO contracts.
- Avoid schema, migration, or posting-flow changes.
- Leave unrelated uncommitted wave-3 and wave-4 changes intact.

## Ordered Steps

1. Replace the settlement spec/delegate containers with smaller grouped models and normalized allocation inputs.
2. Update AP/AR settlement adapters, services, and tests to the new settlement shape.
3. Refactor `ApInvoiceService.CreateAsync` into smaller private methods to reduce complexity.
4. Refactor `AuthService` into smaller registration/login helpers and clean up namespace usage.
5. Run targeted builds/tests, then broader validation.
6. Record validation results and any deferred risks.

## Validation Plan

```powershell
dotnet build OakERP.Application/OakERP.Application.csproj /nr:false /m:1
dotnet build OakERP.Auth/OakERP.Auth.csproj /nr:false /m:1
dotnet build OakERP.Tests.Unit/OakERP.Tests.Unit.csproj /nr:false /m:1
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj /nr:false /m:1
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj /nr:false /m:1
dotnet build OakERP.sln /nr:false /m:1
```
