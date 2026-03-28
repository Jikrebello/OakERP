# Test Harness Cleanup

## Scope

Implement only the approved dead-path cleanup in `OakERP.Tests.Integration`:
- reconfirm `IntegrationTestBase` has no consumers immediately before editing
- remove only the unused `IntegrationTestBase` path
- keep the live integration harness centered on `WebApiIntegrationTestBase`
- do not rename files/classes unless strictly required
- do not touch frameworks, DB reset strategy, seeding behavior, or test project boundaries

## Constraints

- do not change test frameworks or attempt xUnit/NUnit unification
- do not change DB reset strategy
- do not change seeding behavior
- do not modify the integration test project file unless strictly necessary
- run validation sequentially, not in parallel
- clearly separate environment/file-lock noise from real regressions

## Ordered Steps

1. Reconfirm `IntegrationTestBase` has no consumers.
2. Record the active test harness structure and framework split in findings.
3. Delete only `OakERP.Tests.Integration/TestSetup/IntegrationTestBase.cs`.
4. Record that the active integration harness remains `WebApiIntegrationTestBase` + `OakErpWebFactory` + `TestConfiguration` + `TestDatabaseReset`.
5. Run sequential validation commands and document results.

## Validation Plan

1. `dotnet build OakERP.Tests.Integration/OakERP.Tests.Integration.csproj`
2. `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --no-restore`
3. `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --no-restore`
4. `dotnet build OakERP.sln`
