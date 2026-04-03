# Task Plan

## Task Name
posting-remediation-slice

## Goal
Fix the smallest high-value maintainability issues in the completed AR invoice posting slices without redesigning the posting architecture.

## Background
The posting audit found one real migration defect, one runtime-vs-persisted posting model leak, several domain-significant literals that should be centralized, weaker-than-intended posting-result validation, and one misleading fallback test.

## Scope
- `OakERP.Domain` posting/runtime constants and posting rule model cleanup
- `OakERP.Infrastructure` posting, repository, and migration cleanup related to AR invoice posting
- `OakERP.Tests.Unit` and `OakERP.Tests.Integration` posting tests only as needed for this remediation slice
- targeted repo docs and Codex guidance files called out in the audit

## Out of Scope
- new posting slices or document types
- posting architecture redesign
- repository redesign
- schema redesign beyond fixing the existing 1B migration rollback
- auth, UI, or client refactors

## Constraints
- Preserve behavior unless explicitly fixing a defect.
- Keep the change set small and reviewable.
- Centralize only domain-significant literals.
- Do not widen repository responsibilities unless strictly required.
- Update task docs as work progresses.

## Success Criteria
- [ ] 1B migration `Down()` only reverses `Up()`
- [ ] runtime posting model no longer depends on persisted posting entity types
- [ ] approved posting literals are centralized
- [ ] posting result validation is tightened for inventory math and traceability invariants
- [ ] misleading fallback test is replaced with real coverage
- [ ] stale slice wording / dead lookups in touched files are cleaned up
- [ ] targeted posting unit tests pass
- [ ] targeted posting integration tests pass
- [ ] solution build passes
- [ ] migration-specific rollback validation is performed and recorded

## Planned Steps
1. Create task docs and confirm the exact remediation file set.
2. Patch the migration, posting runtime model, constants, and posting validation code.
3. Replace the misleading test with real fallback coverage and adjust related tests.
4. Update the relevant `.md` guidance files with explicit standards.
5. Run CSharpier on touched C# files.
6. Run targeted validation and record results, including rollback validation.

## Validation Commands
```powershell
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter FullyQualifiedName~Posting
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ArInvoicePosting
dotnet build OakERP.sln
```

## Test Notes
This task requires both unit and integration tests because it changes posting validation behavior, test expectations, and migration safety expectations.

## Risks
- Fixing the runtime posting rule type leak may expose any hidden dependencies on the persisted posting namespace.
- Tightening validation may fail tests that were previously passing with underspecified output assumptions.

## Notes
- The migration rollback fix is a defect correction, not a schema redesign.
- The rollback validation step must prove only the `ar_invoice_lines.location_id` change is reversed.
