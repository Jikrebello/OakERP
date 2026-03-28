# Progress

## Started

- Confirmed the slice can stay narrow without changing schema, API contracts, or `ApplicationUser`.
- Confirmed `AuthService` is the primary Identity-manager seam to target.

## Completed

- Added `IIdentityGateway` and `IdentityGateway` in `OakERP.Auth` as the narrow auth-local wrapper over the five Identity operations used by `AuthService`.
- Refactored `AuthService` to depend on `IIdentityGateway` instead of `UserManager<ApplicationUser>` and `SignInManager<ApplicationUser>`.
- Preserved the existing JWT mapping point in `AuthService`: `ApplicationUser` is still mapped to `JwtTokenInput` immediately before token generation.
- Updated `AddAuthServices()` to register the new gateway.
- Updated the unit-test factory and auth service tests to mock `IIdentityGateway` instead of raw Identity managers.

## Validation

- Passed: `dotnet build OakERP.Auth/OakERP.Auth.csproj`
- Passed: `dotnet build OakERP.API/OakERP.API.csproj`
- Passed with existing warning only: `D:\\CSharp_Projects\\OakERP\\OakERP.API\\Controllers\\BaseController.cs(23,57)` nullable warning during targeted API build
- Passed: `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --no-restore`
- Passed: `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --no-restore`
- Passed: `dotnet build OakERP.sln`

## Remaining

- Deeper identity/domain cleanup is still deferred; `ApplicationUser : IdentityUser` remains unchanged.
