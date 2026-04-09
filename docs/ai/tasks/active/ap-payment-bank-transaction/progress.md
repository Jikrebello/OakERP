# Progress

## Task
ap-payment-bank-transaction

## Started
2026-04-09 22:55:21

## Work Log
- Created the task folder and documented the approved scope, polarity correction requirement, and validation plan.
- Corrected the AP payment runtime rule and posting engine output from debit-bank/credit-AP to debit-AP/credit-bank.
- Added outbound bank transaction persistence inside `ApPaymentPostingOperation` using the existing `IBankTransactionRepository` passed through `PostingService`.
- Updated AP payment unit, posting integration, and API integration tests to assert corrected GL polarity, bank transaction persistence, duplicate resistance, and rollback behavior.
- Fixed CSharpier drift surfaced by `tools/validate-pr.ps1`.

## Files Touched
- `OakERP.Application/Posting/Operations/ApPaymentPostingOperation.cs`
- `OakERP.Application/Posting/Services/PostingService.cs`
- `OakERP.Infrastructure/Posting/PostingEngine.cs`
- `OakERP.Infrastructure/Posting/PostingRuleProvider.cs`
- `OakERP.Tests.Unit/Posting/ApPaymentPostingServiceTests.cs`
- `OakERP.Tests.Unit/Posting/PostingEngineApPaymentTests.cs`
- `OakERP.Tests.Unit/Posting/PostingServiceTestFactory.cs`
- `OakERP.Tests.Integration/Posting/ApPaymentPostingTests.cs`
- `OakERP.Tests.Integration/AccountsPayable/ApPaymentApiTests.cs`
- `docs/ai/tasks/active/ap-payment-bank-transaction/task_plan.md`
- `docs/ai/tasks/active/ap-payment-bank-transaction/findings.md`
- `docs/ai/tasks/active/ap-payment-bank-transaction/progress.md`

## Validation
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter "FullyQualifiedName~ApPayment"` passed.
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ApPaymentPostingTests` passed.
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ApPaymentApiTests` passed.
- `pwsh ./tools/validate-pr.ps1` passed after formatting the three AP payment slice files flagged by CSharpier.
- A parallel integration test attempt hit a shared `MvcTestingAppManifest.json` file lock in `obj`; rerunning the two integration classes sequentially resolved that harness-level collision without code changes.

## Remaining
- None for this slice.

## Deferred Smells / Risks
- Reconciliation, statement import, reversal/unposting, FX redesign, and UI work remain out of scope.

## Outcome
- Completed. AP payment posting now tells a consistent outbound-cash story in both GL and the persisted bank transaction row.

## Next Recommended Step
- Use this slice as the stable outbound-cash baseline for future bank reconciliation or statement-matching work, without widening the current posting contracts.
