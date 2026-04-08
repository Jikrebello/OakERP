# Progress

## Task
dto-to-dto-casing

## Started
2026-04-08 13:10:19

## Work Log
- Reproduced the broken state with `dotnet build OakERP.sln`.
- Identified accidental rename fallout in auth and infrastructure method names (`AddToRoleAsync`, `ReadToEnd`, `ReadToEndAsync`).
- Repaired those method names and corresponding test references.
- Renamed the affected DTO files from `*DTO.cs` to `*Dto.cs`.
- Forced the case-only renames through Git so the repo tracks the new casing on Windows.

## Files Touched
- `OakERP.Auth`
- `OakERP.Infrastructure`
- `OakERP.Tests.Integration/Runtime`
- `OakERP.Tests.Unit/Auth`
- DTO files under `OakERP.Application` and `OakERP.Common`

## Validation
- `dotnet build OakERP.sln` passed

## Remaining
- No remaining compile failures from this rename slice.

## Deferred Smells / Risks
- The `OakERP.Common/DTOs` folder name remains as-is.

## Outcome
- The solution builds again and DTO file names now match the `Dto` type names.

## Next Recommended Step
- Continue addressing any additional style or analyzer warnings in smaller batches.
