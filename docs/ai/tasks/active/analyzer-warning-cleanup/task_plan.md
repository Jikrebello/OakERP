# Task Plan

## Task Name
analyzer-warning-cleanup

## Goal
Resolve the user-reported Auth, Common, and Infrastructure analyzer findings without changing OakERP runtime behavior.

## Background
The user is using Visual Studio and Sonar-style diagnostics as the current compliance loop. Several warnings came from dead commented code, static helper classes being injected, oversized constructor signatures, repeated literals, and high-complexity validation methods.

## Scope
- `OakERP.Auth/AuthService.cs`
- `OakERP.Common/Enums/`
- `OakERP.Domain/Entities/*` currency default references
- `OakERP.Infrastructure/Accounts_Payable/`
- `OakERP.Infrastructure/Accounts_Receivable/`
- `OakERP.Infrastructure/Posting/`
- `OakERP.Infrastructure/Extensions/ServiceCollectionExtensions.cs`
- affected unit test factories in `OakERP.Tests.Unit/`

## Out of Scope
- migration behavior or schema changes
- integration-test behavior changes
- API contract changes unrelated to the reported diagnostics
- generated migration analyzer cleanup outside the reported warning set

## Constraints
- Preserve behavior unless explicitly asked otherwise.
- Keep DI changes structural and minimal.
- Do not churn generated migrations unless the user asks for that cleanup explicitly.

## Success Criteria
- [x] The reported diagnostics are addressed in the touched code
- [x] Relevant builds pass
- [x] Relevant tests pass
- [x] Docs updated
- [x] Remaining risks are documented
- [x] Required unit tests updated where constructor/static-helper wiring changed
- [x] Required integration tests intentionally not updated because behavior did not change
- [x] New domain-significant constants / enums documented if introduced
- [x] Deferred smells / risks recorded if intentionally left unresolved

## Planned Steps
1. Remove dead/commented code, rename the ISO currency enum, and make pure helper types explicitly static.
2. Reduce constructor fan-in by removing injected static helpers and grouping only the remaining cohesive service dependencies.
3. Split high-complexity validator/posting methods into focused helpers and rerun build/test/analyzer validation.

## Validation Commands
```powershell
dotnet build OakERP.Auth/OakERP.Auth.csproj
dotnet build OakERP.Infrastructure/OakERP.Infrastructure.csproj
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj
dotnet build OakERP.sln
dotnet format analyzers OakERP.Infrastructure/OakERP.Infrastructure.csproj --no-restore --verify-no-changes --severity info
dotnet format analyzers OakERP.Auth/OakERP.Auth.csproj --no-restore --verify-no-changes --severity info
dotnet format analyzers OakERP.Common/OakERP.Common.csproj --no-restore --verify-no-changes --severity info
```

## Test Notes
- unit tests: yes, because service constructor and static-helper wiring changed
- integration tests: no, because runtime behavior was preserved and no transport/persistence contracts changed

## Risks

- generated migration files still surface unrelated `CA1861` suggestions if analyzer verification is run at `info` severity against the whole Infrastructure project
- Visual Studio may need a refresh to clear stale live-analyzer entries after constructor and enum renames

## Architecture Checks

- Runtime and persisted model boundaries were preserved.
- Repeated posting-rule literal was centralized.
- New dependency bundles were added only where they removed concrete constructor fan-in.
- Thin orchestrators remained orchestration-focused.

## Notes

- Added `PostingRuleRequiredMessage` constant in `PostingEngine`.
- Renamed `CurrencyISOCodes` to `CurrencyIsoCodes` and matched the file name.
