# Progress

## Changed
- added shared `ResultError` and `OakErpException` foundations in `OakERP.Common`
- centralized semantic exceptions in `OakERP.Common.Exceptions`
- added workflow error catalogs for Auth, AP invoice, AP payment, and AR receipt flows
- added `IClock` / `SystemClock` and posting math constants
- converted posting, config, seeding, and embedded-resource failures to typed exceptions
- updated `GlobalExceptionHandler` to translate `OakErpException` into stable problem-details responses
- removed committed dev/prod secrets and localhost defaults from API/Web appsettings, with local override files ignored by git
- updated startup/config binding in API, Web, Desktop, Auth, Infrastructure, and tests
- updated unit/integration tests and test factories for the new exception and configuration seams

## Validation
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj /nr:false /m:1` passed with 106 tests
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj /nr:false /m:1` passed with 73 tests
- `dotnet build OakERP.sln /nr:false /m:1` passed with 0 warnings and 0 errors

## Notes
- parallel project builds hit transient file locks from external processes a few times; rerunning serially confirmed the source tree was healthy
- no raw `InvalidOperationException` / `NotSupportedException` throw sites remain in `OakERP.API`, `OakERP.Auth`, `OakERP.Application`, `OakERP.Infrastructure`, or `OakERP.Client`
