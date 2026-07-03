# Local Auth, CORS, And Test Setup Progress

## Started

- Created task tracking for the local auth/CORS/integration setup slice.

## Changes

- Created separate `AuthApiRoutes` constants for `/api/auth/*` client calls.
- Updated client auth service to use API routes while leaving UI page routes unchanged.
- Fixed login/register view models to return on failed validation and reset busy state with `finally`.
- Updated API startup so ignored local config is not loaded in `Testing`.
- Added tracked API development CORS origins for the Web host.
- Added unit tests for auth API routes and invalid login/register view-model submission.
- Added integration coverage for effective test host auth/database/CORS configuration.
- Added a shared integration helper for posting prerequisites and wired API/lower-level posting helper paths to seed required posting accounts, `gl.posting`, and relevant open fiscal periods.

## Validation

- Passed: `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter FullyQualifiedName~Auth`
- Passed: `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~Auth`
- Passed: `dotnet build OakERP.Web/OakERP.Web.csproj`
- Passed: `dotnet build OakERP.Desktop/OakERP.Desktop.csproj`
- Failed: `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj`
  - The old setup failure is gone; 95 of 109 tests passed.
  - Remaining failures were posting API/test setup prerequisites (`gl.posting` and open fiscal periods), now addressed with explicit test helper setup.
- Passed: `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~Posting`
- Passed: `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter "FullyQualifiedName~ApInvoiceApiTests|FullyQualifiedName~ArInvoiceApiTests|FullyQualifiedName~ApPaymentApiTests|FullyQualifiedName~ArReceiptApiTests"`
- Passed: `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj`
  - 109 passed, 0 failed.
- Re-ran and passed: `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter FullyQualifiedName~Auth`
- Passed: `git diff --check`
- Could not run: `dotnet csharpier check .`
  - `dotnet tool restore` failed while downloading CSharpier 1.2.6.

## Remaining

- No remaining work in this slice.
