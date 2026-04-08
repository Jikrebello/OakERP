---
name: oakerp-client-host-boundaries
description: Client plumbing, shared UI, and host-boundary work for OakERP. Use when moving or refactoring code across OakERP.Client, OakERP.Shared, OakERP.UI, OakERP.Web, OakERP.Desktop, or OakERP.Mobile.
---

# OakERP Client Host Boundaries

Use this skill for client plumbing, shared UI, and host-boundary refactors in OakERP.
Keep shared code narrow, keep hosts host-specific, and avoid letting `OakERP.Shared` become a junk drawer.

## Read First

Inspect:
- `AGENTS.md`
- `docs/architecture/dependency-rules.md`
- `docs/architecture/project-map.md`
- `docs/ai/tasks/archive/shared-client-plumbing-separation/progress.md`
- `docs/ai/tasks/archive/shared-ui-auth-state-separation/progress.md`

Trace the relevant files across:
- `OakERP.Client`
- `OakERP.Shared`
- `OakERP.UI`
- `OakERP.Web`
- `OakERP.Desktop`
- `OakERP.Mobile` only if the task explicitly requires it

## Core Rules

- Put non-UI client plumbing in `OakERP.Client`.
- Keep genuinely shared Razor shell and components in `OakERP.Shared`.
- Keep narrow moved UI-state concerns in `OakERP.UI`.
- Keep host-specific adapters, token stores, and platform services in host projects.
- Avoid widening `OakERP.Mobile` during unrelated work.
- Preserve existing namespaces when that avoids churn without harming ownership.

## Workflow

### 1. Classify The Concern

Decide whether the code is:
- non-UI API or auth plumbing
- shared Razor UI
- view-model or form-state logic
- web-only behavior
- desktop-only behavior
- mobile-only behavior

If the concern is only conveniently shared, it probably does not belong in `OakERP.Shared`.

### 2. Move The Smallest Coherent Slice

When refactoring, update:
- project references
- dependency injection registration
- host-specific adapters
- any tests or build targets touched by the move

Avoid mixing boundary cleanup with visual redesign or unrelated feature work.

### 3. Validate The Affected Hosts

Use the smallest relevant commands first, for example:

```powershell
dotnet build OakERP.Client/OakERP.Client.csproj
dotnet build OakERP.Shared/OakERP.Shared.csproj
dotnet build OakERP.Web/OakERP.Web.csproj
dotnet build OakERP.Desktop/OakERP.Desktop.csproj
```

If runtime behavior changed, run the relevant unit or integration tests as well.

## Review Checklist

- Did the move reduce coupling or just relocate it?
- Is `OakERP.Shared` narrower after the change?
- Did client plumbing stay out of shared Razor UI?
- Did host-specific behavior remain in the right host?
- Was Mobile left alone unless explicitly in scope?
