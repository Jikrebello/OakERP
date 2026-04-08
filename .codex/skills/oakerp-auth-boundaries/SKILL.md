---
name: oakerp-auth-boundaries
description: Auth seam and identity-boundary work for OakERP. Use when changing OakERP.Auth, IdentityGateway, JWT generation inputs, identity service registration, auth tests, or dependency direction between Auth, Infrastructure, and upper layers.
---

# OakERP Auth Boundaries

Use this skill for OakERP auth and identity-boundary changes.
Keep Auth dependent on abstractions and narrow seams instead of letting framework-heavy details leak upward.

## Read First

Inspect:
- `AGENTS.md`
- `docs/architecture/dependency-rules.md`
- `docs/architecture/project-map.md`
- `docs/ai/tasks/archive/auth-boundary-cleanup/progress.md`
- `docs/ai/tasks/archive/auth-identity-gateway-cleanup/progress.md`

Trace the relevant files across:
- `OakERP.Auth`
- `OakERP.Infrastructure/Extensions`
- `OakERP.API`
- `OakERP.Tests.Unit/Auth`
- `OakERP.Tests.Integration/Auth`

## Core Rules

- Keep `OakERP.Auth` dependent on contracts, seams, and auth-local coordination rather than Infrastructure implementations.
- Keep Identity registration in the layer that owns the persistence concern when it is shared across executables.
- Keep `AuthService` and similar orchestrators thin.
- Keep the JWT mapping seam narrow; map `ApplicationUser` to `JwtTokenInput` close to token generation.
- Do not widen `ApplicationUser : IdentityUser` or deeper identity-domain coupling unless the task explicitly asks for it.
- Add or update auth tests when behavior or seams change.

## Workflow

### 1. Audit The Boundary

Check whether the task is really about:
- service registration ownership
- identity-manager wrapping
- token-generation input shaping
- auth application flow
- test seam cleanup

Look for transitive-reference smells, direct Identity framework leakage, or auth code that is acting like a second infrastructure layer.

### 2. Move Only The Needed Seam

Prefer:
- interface or gateway extraction when it removes direct framework coupling
- layer-owned extension methods for registration shared by API and MigrationTool
- targeted test-factory updates alongside seam changes

Avoid:
- changing schema or user inheritance casually
- pushing auth behavior into API controllers
- adding new abstractions that do not remove a concrete dependency problem

### 3. Validate Auth Paths

Start with the narrowest relevant commands, for example:

```powershell
dotnet build OakERP.Auth/OakERP.Auth.csproj
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter FullyQualifiedName~Auth
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~Auth
```

If the change affects registration used by API or MigrationTool, build those projects as well.

## Review Checklist

- Did Auth end up with fewer direct infrastructure or framework dependencies?
- Did the seam stay narrow and local to auth behavior?
- Did tests move with the seam change?
- Was any deeper identity-domain debt left explicit rather than silently extended?
