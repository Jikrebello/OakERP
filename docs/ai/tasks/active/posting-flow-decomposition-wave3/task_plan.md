# Posting Flow Decomposition Wave 3

## Scope

Decompose `OakERP.Application/Posting/PostingService.cs` so `PostingService` becomes a dispatcher over focused internal posting operations while preserving the existing `IPostingService`, `PostCommand`, and `PostResult` contracts.

## Constraints

- Preserve current posting behavior and error messages unless a test forces a correction.
- Keep posting orchestration in `OakERP.Application`.
- Do not change posting DTOs, API contracts, auth behavior, schema, or host wiring.
- Avoid introducing a large inheritance hierarchy or generic abstraction maze.

## Ordered Steps

1. Capture current posting service/test shape and task findings.
2. Extract shared posting seams:
   - transaction/concurrency wrapper
   - posting-result validation
   - GL/inventory persistence
   - fiscal-period/rule/settings lookup helpers where structurally shared
3. Split document-family orchestration into focused internal operations:
   - AP payment
   - AP invoice
   - AR invoice
   - AR receipt
4. Reduce `PostingService` to dispatch only.
5. Add focused unit tests for the new shared seams.
6. Run targeted posting tests, integration tests, then broader solution validation.
7. Record completed work, validation results, and any deferred smells.

## Validation Plan

```powershell
dotnet build OakERP.Application/OakERP.Application.csproj /nr:false
dotnet build OakERP.Tests.Unit/OakERP.Tests.Unit.csproj /nr:false
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj /nr:false
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj /nr:false
dotnet build OakERP.sln /nr:false
```
