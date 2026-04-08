# Solution Hygiene Wave 7

## Scope
- centralize semantic exception types so Application, API, Auth, Infrastructure, and tests stop relying on raw framework exceptions
- centralize repeated workflow error definitions for Auth, AP invoice, AP payment, and AR receipt flows
- add fail-fast configuration validation for startup and options binding
- remove committed dev/prod localhost defaults and secrets from runtime config where practical
- introduce a clock seam and posting math constants for business-significant time and rounding behavior
- preserve external DTO and HTTP shapes

## Constraints
- keep behavior stable unless the new exception taxonomy requires better-typed failures
- do not introduce schema changes or migrations
- keep testing environment bootable under `WebApplicationFactory`

## Ordered Steps
1. Add shared error/result and exception primitives.
2. Replace raw Auth/AP/AR messages with slice-local error catalogs.
3. Add `IClock` and central posting math constants.
4. Convert posting/application workflows to semantic exceptions.
5. Convert startup/config binding to `ConfigurationValidationException`.
6. Update API exception handling to map `OakErpException` consistently.
7. Externalize dev/prod defaults and use local override files.
8. Update unit/integration tests and factories to the new seams.
9. Run targeted builds, test suites, and full solution build.

## Validation Plan
- `dotnet build OakERP.Common/OakERP.Common.csproj /nr:false /m:1`
- `dotnet build OakERP.Application/OakERP.Application.csproj /nr:false /m:1`
- `dotnet build OakERP.Tests.Unit/OakERP.Tests.Unit.csproj /nr:false /m:1`
- `dotnet build OakERP.Tests.Integration/OakERP.Tests.Integration.csproj /nr:false /m:1`
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj /nr:false /m:1`
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj /nr:false /m:1`
- `dotnet build OakERP.sln /nr:false /m:1`
