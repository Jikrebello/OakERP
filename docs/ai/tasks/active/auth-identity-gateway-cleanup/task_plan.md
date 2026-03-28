# Auth Identity Gateway Cleanup

## Scope

Implement a narrow auth-local identity gateway in `OakERP.Auth` so `AuthService` no longer depends directly on `UserManager<ApplicationUser>` or `SignInManager<ApplicationUser>`.

## Constraints

- Preserve `ApplicationUser : IdentityUser`
- Do not change schema, migrations, or EF/Identity persistence
- Do not change public API contracts
- Do not change login/register/tenant/license behavior
- Keep JWT behavior exactly the same
- Do not introduce new projects or assemblies
- Add the gateway interface and adapter in `OakERP.Auth` only

## Steps

1. Add an auth-local gateway interface covering only the Identity operations used by `AuthService`.
2. Add an auth-local adapter that wraps `UserManager<ApplicationUser>` and `SignInManager<ApplicationUser>`.
3. Refactor `AuthService` to depend on the gateway and preserve existing control flow.
4. Update auth DI registration to register the new gateway.
5. Update unit tests to mock the gateway seam instead of the raw Identity managers.
6. Run validation in the agreed order.

## Validation

1. `dotnet build OakERP.Auth/OakERP.Auth.csproj`
2. `dotnet build OakERP.API/OakERP.API.csproj`
3. `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --no-restore`
4. `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --no-restore`
5. `dotnet build OakERP.sln`
