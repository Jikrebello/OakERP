# Task Plan

## Task Name
ar-receipt-bank-transaction

## Goal
Persist the corresponding bank transaction when an AR receipt posts successfully, without changing the external AR receipt post API contract or widening into reconciliation, reversal, FX, or posting redesign.

## Background
AR receipt capture/allocation, AR receipt posting, and AR receipt posting transport already exist. Earlier receipt-posting work deliberately stayed GL-only and deferred bank transaction creation even though the bank transaction entity and repository seams already existed. This task closes that backend gap by adding the persisted bank transaction inside the current AR receipt posting transaction boundary.

## Scope
- `OakERP.Application`
- `OakERP.Infrastructure`
- `OakERP.Tests.Unit`
- `OakERP.Tests.Integration`
- `docs/ai/tasks/active/ar-receipt-bank-transaction`

## Out of Scope
- API route or request/response contract redesign
- bank reconciliation
- statement import or matching
- reversal or unposting
- FX redesign
- AP payment bank transaction creation
- schema or migration changes

## Constraints
- Preserve behavior unless explicitly asked otherwise.
- Do not introduce secrets or hardcoded environment values.
- Keep the change set as small as possible.
- Prefer structural improvement over cosmetic churn.
- Add abstractions only when they solve a real coupling or duplication problem.
- Keep thin orchestrators thin and pure engines/calculators pure when that is part of the intended design.
- Review domain-significant magic numbers and strings.

## Success Criteria
- [ ] AR receipt posting creates exactly one `BankTransaction` row on successful post
- [ ] The bank transaction uses the posted receipt's bank account, posting date, and full receipt amount
- [ ] The bank transaction records `SourceType = PostingSourceTypes.ArReceipt` and `SourceId = receipt.Id`
- [ ] Repeated post attempts do not create duplicate bank transactions
- [ ] Failed receipt posting leaves no committed GL rows or bank transaction rows
- [ ] `IBankTransactionRepository` is registered in infrastructure DI
- [ ] Required unit tests added or updated
- [ ] Required integration tests added or updated
- [ ] Transactional failure / rollback behavior validated
- [ ] No migration is added, and that no-schema decision is documented
- [ ] Deferred follow-on work is recorded explicitly

## Planned Steps
1. Register `IBankTransactionRepository` and extend the posting service wiring only where AR receipt posting needs it.
2. Add bank transaction creation inside `ArReceiptPostingOperation` after posting rows are staged and before the receipt is marked posted.
3. Extend unit and integration tests for success, duplicate resistance, and rollback-on-save-failure behavior.
4. Run targeted validation, then record outcomes and deferred follow-ons in task docs.

## Validation Commands
```powershell
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter FullyQualifiedName~ArReceiptPosting
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter "FullyQualifiedName~ArReceiptPosting|FullyQualifiedName~ArReceiptApiTests"
pwsh ./tools/validate-pr.ps1
```

## Test Notes
State whether this task requires:
- both

## Risks

- save-time failures must still roll back the staged GL rows and the staged bank transaction row together
- the slice must not widen the generic posting runtime contract just to carry bank transaction data

## Architecture Checks

- Are runtime models and persisted entity models still cleanly separated?
- Were business-significant literals centralized instead of repeated?
- Do the tests exercise real fallback paths instead of pre-resolved values?
- Did any new abstraction clearly solve a real problem?
- Did thin orchestrators stay thin and pure engines stay pure where intended?

## Notes

- No schema change is expected because the current `BankTransaction` model/config already has the required columns and filtered unique index on `(SourceType, SourceId)`.
- AP payment bank transaction creation remains the immediate follow-on slice rather than part of this change.
