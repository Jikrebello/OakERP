# Task Plan

## Task Name
dto-to-dto-casing

## Goal
Repair the partial `DTO` to `Dto` casing cleanup so the solution builds again and the DTO file names match their renamed type names.

## Background
A broad find-and-replace changed many DTO type names and namespaces from `DTO` to `Dto`, but it also corrupted unrelated method names such as `AddToRoleAsync` and `ReadToEnd`, and it left multiple file names on the old `*DTO.cs` casing.

## Scope
- `OakERP.Application`
- `OakERP.Auth`
- `OakERP.Common`
- `OakERP.Infrastructure`
- `OakERP.Tests.*`
- task docs for this slice

## Out of Scope
- unrelated behavioral refactors
- runtime contract redesign beyond the casing cleanup
- UI or host changes unrelated to the rename fallout

## Constraints
- Preserve behavior unless explicitly asked otherwise.
- Do not introduce secrets or hardcoded environment values.
- Keep the change set as small as possible.
- Prefer structural improvement over cosmetic churn.
- Add abstractions only when they solve a real coupling or duplication problem.
- Keep thin orchestrators thin and pure engines/calculators pure when that is part of the intended design.
- Review domain-significant magic numbers and strings.

## Success Criteria
- [x] The target issue is addressed
- [x] Relevant build passes
- [ ] Relevant tests pass
- [ ] Docs updated if needed
- [x] Remaining risks are documented
- [ ] Required unit tests added or updated
- [ ] Required integration tests added or updated
- [ ] Migration `Up()` / `Down()` symmetry reviewed if schema work is included
- [ ] New domain-significant constants / enums documented if introduced
- [ ] Transactional failure / rollback behavior validated if persistence behavior changed
- [ ] Deferred smells / risks recorded if intentionally left unresolved

## Planned Steps
1. Reproduce the current build failures from the partial rename.
2. Fix accidental method-name corruption caused by the global replace.
3. Rename DTO file names to `*Dto.cs` so the file system matches the updated type names.
4. Rebuild the solution and record any remaining risks.

## Validation Commands
```powershell
dotnet build OakERP.sln
```

## Test Notes
Build validation is sufficient for this slice because the intended behavior is unchanged and the work is repairing a broken rename plus file-name consistency.

## Risks

- Docs and generated tree files still contain user edits from the same broad rename and were left untouched unless needed for compilation.
- Folder casing such as `DTOs/` remains unchanged; this slice only normalizes type names and file names.

## Architecture Checks

- Are runtime models and persisted entity models still cleanly separated?
- Were business-significant literals centralized instead of repeated?
- Do the tests exercise real fallback paths instead of pre-resolved values?
- Did any new abstraction clearly solve a real problem?
- Did thin orchestrators stay thin and pure engines stay pure where intended?

## Notes

- Preserve the user's in-progress `DTO` to `Dto` direction rather than reverting the naming convention.
