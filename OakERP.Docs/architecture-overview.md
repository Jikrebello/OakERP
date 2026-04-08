# OakERP Architecture Overview

OakERP is being shaped toward clean architecture with explicit host, application, persistence, and shared-UI boundaries.

## High-Level Layers

### Domain

`OakERP.Domain` owns business entities, repository contracts, and runtime posting models.

It should stay free of API, UI host, and infrastructure implementation concerns.

### Application

`OakERP.Application` owns use cases, orchestration, settlements, and posting services.

It coordinates business flows but should not depend on API transport code or infrastructure implementations.

### Infrastructure

`OakERP.Infrastructure` owns EF Core, persistence, repositories, seeding, and data-building support for posting/runtime operations.

### Auth

`OakERP.Auth` owns auth workflows, JWT generation, and identity seams.

It should stay focused on auth/application concerns rather than becoming a second infrastructure layer.

### API

`OakERP.API` is the HTTP transport and runtime/composition layer.

It owns:

- controllers
- Swagger/OpenAPI
- middleware and runtime support
- CORS, rate limiting, timeouts, and health endpoints
- transport-side exception/status behavior

## Client / Host Split

### Client plumbing

`OakERP.Client` owns API/auth/session plumbing that should not live in shared Razor UI code.

### Shared UI shell

`OakERP.Shared` owns the shared Razor shell and shared MAUI host-core adapters.

### UI state

`OakERP.UI` holds narrower UI-state/view-model concerns that do not belong in the shared shell.

### Hosts

- `OakERP.Web`: Blazor web host
- `OakERP.Desktop`: MAUI desktop host
- `OakERP.Mobile`: MAUI mobile host

## Operational Projects

- `OakERP.MigrationTool`: explicit migration/seeding executable
- `OakERP.Tests.Unit`: fast isolated tests
- `OakERP.Tests.Integration`: runtime/API/persistence integration tests
- `OakERP.Docs` and `docs/architecture`: living developer and architecture docs

## Current Direction

Recent refactors have moved the solution toward:

- application-owned orchestration instead of infrastructure-owned business flow
- cleaner auth boundaries
- stronger API transport ownership
- shared client and MAUI host composition
- better exception/config hygiene
- stronger unit/integration/Swagger test coverage

For the normative architecture rules, see:

- [../docs/architecture/dependency-rules.md](../docs/architecture/dependency-rules.md)
- [../docs/architecture/project-map.md](../docs/architecture/project-map.md)
