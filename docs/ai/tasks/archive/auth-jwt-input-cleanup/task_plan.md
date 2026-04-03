## Scope

Implement only the narrow JWT input cleanup slice by replacing `IJwtGenerator.Generate(ApplicationUser)` with a minimal auth-local token input contract, while preserving JWT claim set, token behavior, and public API behavior.

## Constraints

- The JWT input type must stay auth-local and minimal.
- Keep the JWT claim set and token behavior exactly the same.
- Do not move `ApplicationUser`, repository interfaces, or Identity-related persistence types.
- Do not introduce new projects or assemblies.
- If changing `IJwtGenerator` would force controller/API contract changes or broader `AuthService` redesign, stop and report the conflict.

## Ordered Steps

1. Add a minimal auth-local JWT input contract in `OakERP.Auth`.
2. Update `IJwtGenerator` and `JwtGenerator` to use that contract.
3. Map `ApplicationUser` to the auth-local JWT input inside `AuthService`, and nowhere else.
4. Update unit tests to assert against the new JWT input contract.
5. Run targeted builds/tests first, then broader validation.

## Validation Plan

1. `dotnet build OakERP.Auth/OakERP.Auth.csproj`
2. `dotnet build OakERP.API/OakERP.API.csproj`
3. `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --no-restore`
4. `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --no-restore`
5. `dotnet build OakERP.sln`
