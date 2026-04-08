# Progress

## Started

- Opened wave-4 task records for auth namespace normalization.
- Confirmed `ApplicationUser` still uses the historical domain namespace.
- Confirmed EF designer/snapshot files and auth/infrastructure/test code still reference the old CLR type name.

## Completed

- Changed `ApplicationUser` to the `OakERP.Auth` namespace while keeping the file in the Auth project.
- Updated auth, infrastructure, and test code to consume `ApplicationUser` from `OakERP.Auth`.
- Kept `Tenant` and `License` in `OakERP.Domain.Entities.Users`.
- Reconciled tracked EF designer and snapshot metadata from `OakERP.Domain.Entities.Users.ApplicationUser` to `OakERP.Auth.ApplicationUser`.
- Added an architecture assertion that the Domain assembly does not export `ApplicationUser`.
- Confirmed no new migration was required.

## Validation

- `dotnet build OakERP.Auth/OakERP.Auth.csproj /nr:false /m:1`
- `dotnet build OakERP.Infrastructure/OakERP.Infrastructure.csproj /nr:false`
- `dotnet build OakERP.Tests.Unit/OakERP.Tests.Unit.csproj /nr:false /m:1`
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj /nr:false /m:1`
  - Passed: 101
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj /nr:false /m:1`
  - Passed: 72
- `dotnet build OakERP.sln /nr:false /m:1`
  - Passed with 0 warnings and 0 errors
- Code search for `OakERP.Domain.Entities.Users.ApplicationUser`
  - No remaining matches

## Deferred Risks

- None recorded yet for this wave.
