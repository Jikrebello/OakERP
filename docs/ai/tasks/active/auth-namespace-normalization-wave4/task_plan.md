# Auth Namespace Normalization Wave 4

## Scope

Normalize `ApplicationUser` so the identity type is owned by `OakERP.Auth` while preserving runtime auth behavior, EF schema, and API contracts.

## Constraints

- No HTTP or DTO contract changes.
- No schema change is intended.
- Do not move `Tenant` or `License` out of `OakERP.Domain.Entities.Users`.
- Reconcile tracked EF metadata directly unless tooling proves a no-op migration is unavoidable.

## Ordered Steps

1. Record current auth namespace, DI, EF metadata, and test references.
2. Move `ApplicationUser` to the `OakERP.Auth` namespace.
3. Update auth, infrastructure, and test references to the auth-local type.
4. Reconcile EF snapshot and designer metadata strings to the new CLR namespace.
5. Tighten architecture tests so Domain no longer appears to own `ApplicationUser`.
6. Run targeted builds/tests, then broader validation.
7. Record validation results and any deferred risks.

## Validation Plan

```powershell
dotnet build OakERP.Auth/OakERP.Auth.csproj /nr:false
dotnet build OakERP.Infrastructure/OakERP.Infrastructure.csproj /nr:false
dotnet build OakERP.Tests.Unit/OakERP.Tests.Unit.csproj /nr:false
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj /nr:false
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj /nr:false
dotnet build OakERP.sln /nr:false
```
