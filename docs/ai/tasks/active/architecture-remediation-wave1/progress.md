# Progress

## Task
architecture-remediation-wave1

## Started
2026-04-08 16:45:00

## Work Log
- Created task folder and recorded initial scope and constraints.
- Moved AP invoice, AP payment, AR receipt, and posting orchestration from Infrastructure into Application.
- Added an application-safe persistence failure classifier seam and kept provider-specific exception detection in Infrastructure.
- Relocated `ApplicationUser` out of the Domain assembly into Auth while preserving its namespace for EF compatibility.
- Replaced raw JWT config reads with validated `JwtOptions`.
- Unified Web/Desktop API client registration around `ApiClientOptions` and explicit client/UI host composition.
- Removed hardcoded design-time and integration-test connection string fallbacks in code.
- Added architecture and configuration tests to enforce layering and typed-options wiring.

## Files Touched
- `OakERP.API/Program.cs`
- `OakERP.Application/AccountsPayable/*`
- `OakERP.Application/AccountsReceivable/*`
- `OakERP.Application/Extensions/ServiceCollectionExtensions.cs`
- `OakERP.Application/Interfaces/Persistence/IPersistenceFailureClassifier.cs`
- `OakERP.Application/Posting/*`
- `OakERP.Application/OakERP.Application.csproj`
- `OakERP.Auth/ApplicationUser.cs`
- `OakERP.Auth/Extensions/ServiceCollectionExtensions.cs`
- `OakERP.Auth/JwtGenerator.cs`
- `OakERP.Auth/JwtOptions.cs`
- `OakERP.Auth/OakERP.Auth.csproj`
- `OakERP.Client/ApiClientOptions.cs`
- `OakERP.Client/Extensions/ServiceCollectionExtensions.cs`
- `OakERP.Desktop/MauiProgram.cs`
- `OakERP.Domain/OakERP.Domain.csproj`
- `OakERP.Infrastructure/Extensions/ServiceCollectionExtensions.cs`
- `OakERP.Infrastructure/OakERP.Infrastructure.csproj`
- `OakERP.Infrastructure/Persistence/DesignTimeDbContextFactory.cs`
- `OakERP.Infrastructure/Persistence/PersistenceFailureClassifier.cs`
- `OakERP.Shared/Extensions/ServiceCollectionExtensions.cs`
- `OakERP.Tests.Integration/TestSetup/OakErpTestDatabaseOptions.cs`
- `OakERP.Tests.Integration/TestSetup/TestConfiguration.cs`
- `OakERP.Tests.Integration/TestSetup/TestDatabaseReset.cs`
- `OakERP.Tests.Unit/Architecture/DependencyRulesTests.cs`
- `OakERP.Tests.Unit/Configuration/OptionsBindingTests.cs`
- `OakERP.Tests.Unit/AccountsPayable/*`
- `OakERP.Tests.Unit/AccountsReceivable/*`
- `OakERP.Tests.Unit/Posting/PostingServiceTestFactory.cs`
- `OakERP.Tests.Unit/OakERP.Tests.Unit.csproj`
- `OakERP.Web/Program.cs`
- `docs/ai/tasks/active/architecture-remediation-wave1/task_plan.md`
- `docs/ai/tasks/active/architecture-remediation-wave1/findings.md`
- `docs/ai/tasks/active/architecture-remediation-wave1/progress.md`

## Validation
- `dotnet build OakERP.Auth/OakERP.Auth.csproj /nr:false`
- `dotnet build OakERP.Infrastructure/OakERP.Infrastructure.csproj /nr:false`
- `dotnet build OakERP.Web/OakERP.Web.csproj /nr:false`
- `dotnet build OakERP.Desktop/OakERP.Desktop.csproj /nr:false`
- `dotnet build OakERP.Tests.Unit/OakERP.Tests.Unit.csproj /nr:false`
- `dotnet build OakERP.sln /nr:false`
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj /nr:false`
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj /nr:false`

## Remaining
- Mobile host alignment is still intentionally minimal in this wave.
- Full namespace normalization for the moved identity model is deferred.

## Deferred Smells / Risks
- `ApplicationUser` still uses the historical Domain namespace for compatibility, even though the type now lives in Auth.
