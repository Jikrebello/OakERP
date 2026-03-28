## Progress

- Created task folder for the auth JWT input cleanup slice.
- Confirmed `IJwtGenerator` is only consumed in `AuthService` and unit tests.
- Confirmed the current JWT claim set depends only on `Id`, `Email`, and `TenantId`.
- Confirmed the correct mapping point is inside `AuthService`.
- Added a minimal auth-local `JwtTokenInput` contract in `OakERP.Auth`.
- Updated `IJwtGenerator` and `JwtGenerator` to use `JwtTokenInput` instead of `ApplicationUser`.
- Added a single private mapping helper inside `AuthService` to map `ApplicationUser -> JwtTokenInput`.
- Updated unit tests to mock and verify `IJwtGenerator.Generate(JwtTokenInput)`, including a success-path assertion of the mapped values.
- Sequential validation passed:
  - `dotnet build OakERP.Auth/OakERP.Auth.csproj`
  - `dotnet build OakERP.API/OakERP.API.csproj`
  - `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --no-restore`
  - `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --no-restore`
  - `dotnet build OakERP.sln`

## Validation Notes

- The targeted API build surfaced the existing nullable warning in `OakERP.API/Controllers/BaseController.cs`; this slice did not modify API contracts or controller behavior.
- No new build or test regressions were introduced by the JWT input cleanup.

## Pending

- No remaining work in this slice.
