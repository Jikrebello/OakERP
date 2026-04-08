# Findings

## Task
dto-to-dto-casing

## Current State
A broad manual `DTO` to `Dto` replacement already changed many type names, namespaces, comments, and usages across the repo. The compile break was not caused by the DTO types themselves; it was caused by accidental replacements inside unrelated method names and framework methods.

## Relevant Projects
- `OakERP.Application`
- `OakERP.Auth`
- `OakERP.Common`
- `OakERP.Infrastructure`
- `OakERP.Tests.Integration`
- `OakERP.Tests.Unit`

## Dependency Observations
- No architectural dependency issue was introduced by the casing change itself.
- The fallout is limited to symbol spelling and path consistency.
- 

## Structural Problems
- Several DTO type files still used the old `*DTO.cs` filenames after the internal type names were changed to `*Dto`.
- A global replace corrupted method names including `AddToRoleAsync` and `ReadToEnd`.
- 

## Literal / Model-Family Notes
- Repeated business-significant literals:
- Repeated domain-significant numbers:
- Runtime-vs-persisted model-family conflicts:
- Thin orchestrators getting too thick:
- Pure engines/calculators with side effects or lookups:

## Configuration / Environment Notes
- None for this slice.
- 

## Testing Notes
- Full solution build is the relevant validation because the breakage surfaced at compile time.
- 

## Rollback / Transaction Notes
- Migration rollback reviewed:
- Transactional failure leaves no writes:

## Open Questions
- None after reproducing the build failures.
- 

## Deferred Smells / Risks
- The `OakERP.Common/DTOs/` folder name still uses the old acronym casing; the current request only asked to make file names match.
- 

## Recommendation
Fix the accidental symbol corruption, rename the DTO files to `*Dto.cs`, and stop there.

