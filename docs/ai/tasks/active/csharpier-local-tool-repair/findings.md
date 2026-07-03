# Findings

## Task
csharpier-local-tool-repair

## Current State
- The repo pins CSharpier as a local .NET tool at version `1.2.6`.
- `dotnet tool restore` succeeded.
- Before the tool cache was warmed, `dotnet csharpier --version` reported that the tool needed restore even though restore had completed.
- `dotnet tool run csharpier -- --version` warmed the resolver cache and returned `1.2.6`.
- After that, `dotnet csharpier --version` also returned `1.2.6`.

## Relevant Projects
- `OakERP.API`
- `OakERP.Mobile`
- `OakERP.Tests.Integration`
- `OakERP.Tests.Unit`

## Formatter Drift
`dotnet csharpier check .` reported drift in:
- `OakERP.API/Program.cs`
- `OakERP.Mobile/OakERP.Mobile.csproj`
- `OakERP.Tests.Integration/Auth/AuthApiTests.cs`
- `OakERP.Tests.Integration/Posting/ApInvoicePostingTests.cs`
- `OakERP.Tests.Integration/Posting/ApPaymentPostingTests.cs`
- `OakERP.Tests.Unit/OakERP.Tests.Unit.csproj`

## Dependency Observations
- No dependency direction changes were needed.
- No new dependencies or abstractions were introduced.

## Configuration / Environment Notes
- The issue was local formatter/tooling state plus formatter drift, not application runtime configuration.
- Visual Studio may still require a restart after the local tool cache is warmed.

## Testing Notes
- Formatter-only task; no behavior tests are required.

## Rollback / Transaction Notes
- Migration rollback reviewed: not applicable.
- Transactional failure leaves no writes: not applicable.

## Deferred Smells / Risks
- If Visual Studio's CSharpier extension remains broken after restart, the remaining issue is likely extension installation/cache state outside the repo.

## Recommendation
Keep the pinned CSharpier version and use the restored local tool path. Restart Visual Studio after this formatter baseline is clean.
