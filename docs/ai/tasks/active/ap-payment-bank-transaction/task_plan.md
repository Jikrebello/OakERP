# Task Plan

## Task Name
ap-payment-bank-transaction

## Goal
Correct AP payment posting polarity to outbound-cash semantics and persist one matching bank transaction for each successful posted AP payment inside the existing posting transaction.

## Background
OakERP already supported AP payment create/capture, allocation, posting, and posting transport. The remaining backend gap was the missing bank transaction row on successful outbound cash posting. During planning, the current AP payment GL shape was also identified as internally inconsistent with withdrawal semantics, so this slice includes the minimum polarity correction needed to align GL and bank transaction behavior.

## Scope
- `OakERP.Application/Posting`
- `OakERP.Infrastructure/Posting`
- `OakERP.Tests.Unit/Posting`
- `OakERP.Tests.Integration/Posting`
- `OakERP.Tests.Integration/AccountsPayable`
- `docs/ai/tasks/active/ap-payment-bank-transaction`

## Out of Scope
- Bank reconciliation
- Bank statement import or matching
- Unposting or reversal
- FX redesign
- AR receipt behavior
- UI, web, desktop, or mobile flows
- Broad posting architecture changes

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
- [x] Relevant tests pass
- [x] Docs updated if needed
- [x] Remaining risks are documented
- [x] Required unit tests added or updated
- [x] Required integration tests added or updated
- [x] Migration `Up()` / `Down()` symmetry reviewed if schema work is included
- [x] New domain-significant constants / enums documented if introduced
- [x] Transactional failure / rollback behavior validated if persistence behavior changed
- [x] Deferred smells / risks recorded if intentionally left unresolved

## Planned Steps
1. Correct the AP payment runtime rule and posting engine output to `Dr AP control / Cr Bank`.
2. Add AP payment bank transaction persistence inside `ApPaymentPostingOperation` using the existing `IBankTransactionRepository`.
3. Update unit and integration tests to assert corrected GL polarity, bank transaction persistence, and rollback behavior.
4. Validate with targeted unit/integration runs and `tools/validate-pr.ps1`.

## Validation Commands
```powershell
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter "FullyQualifiedName~ApPayment"
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ApPaymentPostingTests
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ApPaymentApiTests
pwsh ./tools/validate-pr.ps1
```

## Test Notes
This task required both unit and integration tests because it changes posting output, persisted side effects, and transactional rollback behavior.

## Risks

- Stale AP payment fixtures or GL assertions could have preserved the old polarity in tests while runtime behavior changed. This slice updated the runtime rule provider, posting engine, test factory rule fixture, engine assertions, service assertions, and integration assertions together.
- Save-time rollback needed revalidation because the slice adds a second persisted artifact to AP payment posting. The integration suite now includes a save-failure rollback case driven by the existing `ck_banktxn_dr_neq_cr` constraint.

## Architecture Checks

- Runtime models and persisted entity models remain separated: GL output is still produced by the posting engine; the persisted bank transaction is created in the posting operation.
- Business-significant AP payment polarity literals were updated together in runtime and test fixtures.
- Tests exercise real runtime paths, including save-time database rollback.
- No new abstraction was introduced.
- `PostingService` stayed a thin orchestrator and `PostingEngine` stayed pure.

## Notes

- No schema change was needed; the existing `BankTransaction` model, EF configuration, filtered unique index on `(SourceType, SourceId)`, and integrity checks were sufficient.
- The slice intentionally mirrors the AR receipt bank transaction insertion point while correcting AP payment polarity to match outbound cash.
