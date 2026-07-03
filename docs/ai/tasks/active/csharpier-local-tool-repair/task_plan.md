# CSharpier Local Tool Repair

## Goal
Restore local CSharpier behavior so the Visual Studio extension and repo validation have a clean formatter baseline.

## Background
Visual Studio's CSharpier extension was installed but crashing. The repo uses a pinned local CSharpier tool, so the first check was to verify the local tool restore/cache and then remove any formatter drift that could make the extension fail during formatting.

## Scope
- CSharpier local tool restore and validation from the repo root.
- Files reported by `dotnet csharpier check .`.
- Task notes for the tooling fix.

## Out of Scope
- Changing CSharpier versions.
- Changing Visual Studio extension installation files.
- Broad formatting churn beyond files reported by CSharpier.
- Behavioral code changes.

## Constraints
- Preserve runtime behavior.
- Keep the change mechanical and reviewable.
- Do not add new dependencies or formatter configuration unless required.

## Success Criteria
- [x] Local CSharpier command resolves from the repo root.
- [x] `dotnet csharpier check .` passes.
- [x] Formatter drift reported by CSharpier is normalized.
- [x] Remaining risks are documented.
- [x] No unit or integration tests added because this is formatter/tooling-only.
- [x] No schema, migration, posting, or transactional behavior changed.

## Planned Steps
1. Verify the pinned local CSharpier tool can restore and run.
2. Format only files reported by CSharpier.
3. Rerun the repo-wide CSharpier check.
4. Run a lightweight build validation around touched projects.

## Validation Commands
```powershell
dotnet tool restore
dotnet csharpier --version
dotnet csharpier check .
dotnet build OakERP.API/OakERP.API.csproj
dotnet build OakERP.Tests.Integration/OakERP.Tests.Integration.csproj
dotnet build OakERP.Tests.Unit/OakERP.Tests.Unit.csproj
```

## Test Notes
No unit or integration tests are required for this task because the changes are formatter-only and do not alter behavior.

## Risks
- Visual Studio may need a restart to pick up the warmed local tool cache.
- If the extension itself is corrupted, reinstalling the extension would be separate from the repo fix.
