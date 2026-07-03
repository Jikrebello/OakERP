# Local Auth, CORS, And Test Setup Plan

## Scope

- Fix local Web/Desktop auth calls without changing API contracts.
- Keep Mobile out of scope.
- Prevent ignored local API config from breaking integration-test database setup.
- Move non-secret local Web CORS origins into tracked development config.

## Constraints

- Preserve existing UI page routes.
- Do not remove ignored `appsettings.Local.json` files.
- Do not introduce secrets.
- Keep changes small and reversible.

## Steps

1. Split client auth API routes from UI page routes.
2. Stop login/register view models when form validation fails and reset busy state reliably.
3. Prevent the API host from loading local JSON while running in `Testing`.
4. Add tracked API development CORS origins for the Web host.
5. Add focused unit/integration coverage.
6. Run targeted validation for auth, Web, and Desktop.

## Validation Plan

```powershell
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter FullyQualifiedName~Auth
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~Auth
dotnet build OakERP.Web/OakERP.Web.csproj
dotnet build OakERP.Desktop/OakERP.Desktop.csproj
```
