# Progress

## Task
posting-remediation-slice

## Completed
- Created the remediation task folder and initial task docs.
- Fixed the 1B migration rollback so `Down()` only reverses the AR invoice line location change.
- Removed the runtime posting rule model dependency on persisted posting entity types.
- Centralized the approved posting constants for AR invoice source type, runtime rule scopes, GL posting settings key, and fiscal period open status.
- Tightened posting result validation for inventory math and traceability invariants.
- Replaced the misleading revenue fallback test with real builder-level fallback coverage.
- Cleaned stale slice wording and dead rule lookups in the touched posting files.
- Updated repo guidance files and task templates with the new standards.

## In Progress
- Final review and close-out.

## Pending
- None.

## Validation
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter FullyQualifiedName~Posting` passed
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ArInvoicePosting` passed
- `dotnet build OakERP.sln` passed
- Migration rollback validation passed via `dotnet ef migrations script 20260329151445_AddArInvoiceLineLocation 20251201170038_Init --project OakERP.Infrastructure/OakERP.Infrastructure.csproj --startup-project OakERP.API/OakERP.API.csproj`, and the generated rollback script only dropped the AR invoice line location FK, index, and column
- Initial rollback validation attempt with `OakERP.MigrationTool` as the startup project failed because that project does not reference `Microsoft.EntityFrameworkCore.Design`

## Notes
- The task is intentionally limited to the issues called out in the audit.
- CSharpier was run on the touched C# files after the code changes.
