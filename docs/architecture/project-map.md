# OakERP Project Map

## Purpose
This file is the short human-readable map of OakERP’s current project layout.

Use this alongside:
- `AGENTS.md`
- `docs/architecture/dependency-rules.md`
- `project-structure.txt`

`project-structure.txt` is the generated tree.
This file explains what the main projects are supposed to own.

## Core Backend Layers

### OakERP.Domain
Business core.
Owns domain entities and core business concepts.
Should avoid infrastructure, API, EF Core, and host/UI concerns where possible.
`OakERP.Domain.Posting` owns runtime posting contracts and models only. It should not silently absorb persisted posting entity types.

### OakERP.Application
Application/use-case layer.
Owns application-facing contracts and orchestration logic.
Should not depend on API or infrastructure implementations.
Thin orchestration is preferred here when a use case is meant to coordinate existing domain/runtime components rather than own detailed business calculations.

### OakERP.Infrastructure
Persistence and external integration layer.
Owns EF Core, DbContext, repositories, seeding infrastructure, and related registration.
Implements backend technical concerns used by API and MigrationTool.
Posting preparation services that resolve data, accounts, or costs may live here, but posting engines should stay pure and repository methods should remain narrow.
Small adapter or preparation components are acceptable here when they keep upper-layer orchestration thin and keep pure runtime engines free of lookups and side effects.

### OakERP.Auth
Authentication/application-auth layer.
Owns auth services, JWT generation, auth-local seams, and identity gateway abstractions.
Should not become a second infrastructure layer or a second user model.

### OakERP.API
HTTP transport and composition layer.
Owns controllers, HTTP pipeline, Swagger, and API-specific configuration.
Should not be the home of reusable cross-executable registration.

### OakERP.MigrationTool
Explicit operational migration/seeding executable.
Used for controlled schema/application startup support outside the main API host.

## Client / UI Layers

### OakERP.Client
Non-UI client plumbing.
Owns client-side API/auth/session/service plumbing that should not live in shared Razor/UI code.

### OakERP.UI
UI-state layer.
Currently owns narrow auth-related UI state and view-model concerns moved out of `OakERP.Shared`.
Should remain narrow unless a broader UI modularization is deliberately approved.

### OakERP.Shared
Shared Razor/UI shell.
Owns genuinely shared UI concerns used across hosts.
Should not become a junk drawer for client plumbing or feature-specific state.

### OakERP.Web
Web host.
Composes shared UI and client services for the web experience.

### OakERP.Desktop
Desktop host.
Composes shared UI and client services for MAUI desktop targets.

### OakERP.Mobile
Mobile host.
Currently more limited than Web/Desktop and should not be widened casually during unrelated refactors.

## Supporting Projects

### OakERP.Common
Cross-cutting primitives and shared contracts stable enough to be reused broadly.
Should stay small and boring.

### OakERP.Docs
Documentation-related project area.
Not the home for repo-control files such as Codex skills, architecture rules, or task templates.

## Test Projects

### OakERP.Tests.Unit
Fast isolated tests.
Used for service-level and seam-level behavior checks.

### OakERP.Tests.Integration
Integration tests against the active API/test harness path.
Covers end-to-end behavior that depends on runtime wiring and persistence.

## Current Architectural Direction
Recent refactors have intentionally moved OakERP toward:
- cleaner dependency direction
- config externalization
- explicit migration/seeding ownership
- narrower auth seams
- separation of client plumbing from shared UI
- separation of auth UI state from the shared shell
- cleaner test and tasking hygiene

## Known Deferred Areas
These are still important but deliberately not fully solved yet:
- deeper Identity/domain coupling
- broader shared UI shell cleanup
- larger feature/module decomposition
- fuller Mobile alignment
- possible future test framework consolidation

## Rule Of Thumb
If a responsibility is:
- non-UI client plumbing -> `OakERP.Client`
- auth-local application/auth seam -> `OakERP.Auth`
- shared shell/UI composition -> `OakERP.Shared`
- narrow moved UI-state concern -> `OakERP.UI`
- persistence/integration/runtime data access -> `OakERP.Infrastructure`
