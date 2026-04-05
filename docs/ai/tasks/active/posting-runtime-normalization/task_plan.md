# Task Plan

## Task Name
posting-runtime-normalization

## Goal
Normalize the shared posting runtime naming and placement after AR invoice posting, AR receipt posting, and AP invoice posting, without changing business behavior.

## Background
The shared posting contracts already live in `OakERP.Domain.Posting`, and document-specific contexts/builders already live in AR/AP folders. The remaining debt is that the single shared concrete engine and rule-provider still live under `OakERP.Infrastructure.Posting.Accounts_Receivable` with AR-biased names even though they now support AP invoice posting too.

## Scope
- `OakERP.Infrastructure`
- `OakERP.Tests.Unit`
- `docs/ai/tasks/active/posting-runtime-normalization/`

## Out of Scope
- business behavior changes
- schema or migration changes
- new posting document types
- posting API/entrypoint redesign
- reversal or unposting work
- `IPostingEngine` or `IPostingRuleProvider` redesign
- `PostingService` split into handlers/services
- generic context-builder abstractions

## Constraints
- Rename and re-home only the shared concrete runtime implementation:
  - `ArPostingEngine` -> `PostingEngine`
  - `ArPostingRuleProvider` -> `PostingRuleProvider`
- Move those implementations to `OakERP.Infrastructure.Posting`.
- Keep all document-specific contexts/builders and posting behavior unchanged.
- Normalize only the misleading shared validation text in `PostingService`.
- Update DI registration, imports, and direct test references.

## Success Criteria
- [x] Shared concrete runtime engine and rule-provider use shared names and live under `OakERP.Infrastructure.Posting`
- [x] DI registration resolves `IPostingEngine` and `IPostingRuleProvider` through the renamed shared types
- [x] Direct test references and imports are updated to the shared names
- [x] `PostingService` shared validation text no longer claims AR-invoice-specific inventory behavior
- [x] Required validation passes and task docs record the normalization and deferrals

## Planned Steps
1. Create task docs and record the current-state shared-runtime findings.
2. Rename and re-home the shared concrete posting engine and rule-provider.
3. Update DI wiring, namespaces, and direct unit test references to the new shared names.
4. Normalize the remaining shared validation text in `PostingService`.
5. Run the required validation suite and update `progress.md`.

## Validation Commands
```powershell
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter FullyQualifiedName~Posting
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ArInvoicePosting
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ArReceiptPosting
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ApInvoicePosting
dotnet build OakERP.API/OakERP.API.csproj
pwsh ./tools/validate-pr.ps1
```
