# Findings

## Current Test Structure

- `OakERP.Tests.Unit`
  - uses xUnit
  - currently contains `AuthService` tests only
  - active helper: `AuthServiceTestFactory`

- `OakERP.Tests.Integration`
  - uses NUnit
  - currently contains auth API integration tests only
  - active harness:
    - `WebApiIntegrationTestBase`
    - `OakErpWebFactory`
    - `TestConfiguration`
    - `TestDatabaseReset`

## Reconfirmed Dead Path

Immediate pre-edit repo-wide search for `IntegrationTestBase` found only:
- `OakERP.Tests.Integration/TestSetup/IntegrationTestBase.cs`

No derived classes or consumers were found.

## Intentional Non-Changes For This Slice

- no xUnit/NUnit unification
- no DB reset strategy change
- no seeding behavior change
- no project-boundary cleanup
- no renames of active test harness classes/files
