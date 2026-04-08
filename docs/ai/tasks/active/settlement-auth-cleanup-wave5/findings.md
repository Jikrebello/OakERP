# Findings

## Current State

- The settlement helpers introduced in wave 2 rely on large generic spec types with many delegate parameters.
- `SettlementAllocationApplicator.ApplyAsync` and the related spec classes trigger analyzer warnings because of their generic count and constructor size.
- `ApInvoiceService.CreateAsync` still owns validation, lookup, uniqueness checks, transaction orchestration, and entity construction in one method.
- `AuthService` still works, but registration and login each carry several concerns inline, making the class harder to scan than it needs to be.

## Risks

- Settlement changes touch both AP and AR allocation flows, so snapshot/unit behavior must stay identical.
- Auth refactoring must preserve audit log payloads and failure messages because tests assert them.
- Existing uncommitted posting/auth namespace work is already in the worktree, so this wave must be additive and non-destructive.
