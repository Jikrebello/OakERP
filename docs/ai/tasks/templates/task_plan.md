# Task Plan

## Task Name
<replace-me>

## Goal
Describe the exact outcome this task should achieve.

## Background
Why this task exists and what problem it is solving.

## Scope
List the projects, folders, and files allowed to change.

## Out of Scope
List what must not be changed as part of this task.

## Constraints
- Preserve behavior unless explicitly asked otherwise.
- Do not introduce secrets or hardcoded environment values.
- Keep the change set as small as possible.
- Prefer structural improvement over cosmetic churn.
- Add abstractions only when they solve a real coupling or duplication problem.
- Keep thin orchestrators thin and pure engines/calculators pure when that is part of the intended design.
- Review domain-significant magic numbers and strings.

## Success Criteria
- [ ] The target issue is addressed
- [ ] Relevant build passes
- [ ] Relevant tests pass
- [ ] Docs updated if needed
- [ ] Remaining risks are documented
- [ ] Required unit tests added or updated
- [ ] Required integration tests added or updated
- [ ] Migration `Up()` / `Down()` symmetry reviewed if schema work is included
- [ ] New domain-significant constants / enums documented if introduced
- [ ] Transactional failure / rollback behavior validated if persistence behavior changed
- [ ] Deferred smells / risks recorded if intentionally left unresolved

## Planned Steps
1.
2.
3.

## Validation Commands
```powershell
dotnet restore
dotnet build
dotnet test
```

## Test Notes
State whether this task requires:
- unit tests
- integration tests
- both
- neither (with reason)

## Risks

- 
- 

## Architecture Checks

- Are runtime models and persisted entity models still cleanly separated?
- Were business-significant literals centralized instead of repeated?
- Do the tests exercise real fallback paths instead of pre-resolved values?
- Did any new abstraction clearly solve a real problem?
- Did thin orchestrators stay thin and pure engines stay pure where intended?

## Notes

- 
- 
