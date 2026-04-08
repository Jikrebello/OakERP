---
name: oakerp-posting
description: Posting runtime and posting-flow work for OakERP across Domain, Application, Infrastructure, migrations, and tests. Use when changing posting engines, posting services, posting context builders, posting rule providers, posting constants, or posting-related test coverage.
---

# OakERP Posting

Use this skill for focused OakERP posting work.
Keep the slice narrow and preserve the current separation between runtime posting models, persisted posting entities, orchestration, and data-loading helpers.

## Read First

Inspect:
- `AGENTS.md`
- `docs/architecture/dependency-rules.md`
- `docs/architecture/project-map.md`
- `docs/ai/tasks/archive/posting-remediation-slice/progress.md`
- any active posting task folder under `docs/ai/tasks/active/`

Trace the relevant files across:
- `OakERP.Application/Posting`
- `OakERP.Domain/Posting`
- `OakERP.Infrastructure/Posting`
- `OakERP.Tests.Unit/Posting`
- `OakERP.Tests.Integration/Posting`

## Core Rules

- Keep runtime posting models separate from persisted posting entity types.
- Keep pure posting engines deterministic and free of lookups, repositories, and transport concerns.
- Keep orchestration services thin when they are meant to coordinate loading, validation, execution, persistence, and transaction boundaries.
- Keep data preparation, account resolution, and settings lookup in narrow infrastructure-side builders or providers when they need persistence access.
- Centralize business-significant posting literals such as source types, rule scopes, settings keys, and status codes.
- Do not claim fallback coverage in tests unless the setup genuinely drives the fallback path.
- If a task touches migrations, make sure each `Down()` only reverses that migration's `Up()`.

## Workflow

### 1. Audit The Slice

Identify which category the task belongs to:
- runtime contracts or result models
- posting engine behavior
- context builder or provider behavior
- orchestration and transaction behavior
- persisted rule or settings mapping
- tests or rollback coverage

Check whether the current change risks:
- mixing runtime and persisted model families
- moving side effects into a pure engine
- duplicating source-type or rule-scope literals
- hiding fallback behavior behind pre-resolved test setup

### 2. Edit The Smallest Layer That Owns The Change

Prefer:
- `OakERP.Domain/Posting` for runtime contracts, constants, and pure engine rules
- `OakERP.Application/Posting` for command and result contracts
- `OakERP.Infrastructure/Posting` for builders, providers, persistence-backed resolution, and orchestration adapters
- tests in the narrowest posting test project that proves the change

Avoid moving posting logic into API controllers, migration scripts, or unrelated repositories.

### 3. Validate With Posting-Focused Coverage First

Start with the smallest relevant validation, for example:

```powershell
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter FullyQualifiedName~Posting
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~Posting
dotnet build OakERP.sln
```

If a task changes rollback behavior, also validate the rollback path directly instead of assuming it.

## Review Checklist

- Did the change preserve runtime versus persisted model separation?
- Did the change keep pure engines pure?
- Did repeated posting literals move into shared constants or enums where appropriate?
- Did the tests prove real fallback or transaction behavior?
- Did task docs record any deferred compromise plainly?
