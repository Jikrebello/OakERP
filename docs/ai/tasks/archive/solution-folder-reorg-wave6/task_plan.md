# Solution Folder Reorganization Wave 6

## Scope

Reorganize the solution physically and by namespace according to the approved whole-solution folder plan.

## Constraints

- No behavior, schema, API contract, or DTO wire-shape changes.
- Keep changes structural only.
- Preserve current runtime composition and test behavior.
- Execute in reviewable waves with validation after each major slice.

## Ordered Steps

1. Normalize naming and reorganize `OakERP.Auth`.
2. Reorganize `OakERP.Application` into operation slices and ordered support folders.
3. Reorganize `OakERP.Common` and `OakERP.Client`.
4. Normalize `OakERP.Domain` and `OakERP.Infrastructure` folder names and namespaces.
5. Move `OakERP.Tests.Integration/ApiRoutes.cs` into `TestSetup` and sweep remaining namespace fallout.
6. Remove empty legacy folders and run full validation.

## Validation Plan

```powershell
dotnet build OakERP.Auth/OakERP.Auth.csproj /nr:false /m:1
dotnet build OakERP.Application/OakERP.Application.csproj /nr:false /m:1
dotnet build OakERP.Common/OakERP.Common.csproj /nr:false /m:1
dotnet build OakERP.Client/OakERP.Client.csproj /nr:false /m:1
dotnet build OakERP.Domain/OakERP.Domain.csproj /nr:false /m:1
dotnet build OakERP.Infrastructure/OakERP.Infrastructure.csproj /nr:false /m:1
dotnet build OakERP.Tests.Unit/OakERP.Tests.Unit.csproj /nr:false /m:1
dotnet build OakERP.Tests.Integration/OakERP.Tests.Integration.csproj /nr:false /m:1
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj /nr:false /m:1
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj /nr:false /m:1
dotnet build OakERP.sln /nr:false /m:1
```
