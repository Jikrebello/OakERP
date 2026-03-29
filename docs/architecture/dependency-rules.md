# Dependency Rules

## Purpose

This document defines the intended dependency boundaries for OakERP so refactors can move the codebase toward cleaner architecture without losing the parts that already work.

These rules are normative for new work and refactor work.

## High-Level Shape

Target direction:

- **Domain** holds core business concepts and rules.
- **Application** holds use cases, orchestration contracts, and application-facing abstractions.
- **Infrastructure** implements persistence and external integration details.
- **Auth** should sit at the edge and depend on abstractions/contracts rather than forcing Infrastructure upward.
- **API** is an HTTP transport layer and composition root.
- **MigrationTool** is an operational executable, not an architectural owner.
- **Web/Desktop/Mobile** are host projects.
- **Shared client/UI code** should support hosts without absorbing every concern in the solution.

## Allowed Dependencies

### Domain

Allowed:
- `OakERP.Common` for shared primitives that are truly cross-cutting and domain-safe

Not allowed:
- API
- Infrastructure
- EF Core
- ASP.NET Identity base classes unless explicitly documented as a temporary exception
- UI host projects
- transport concerns

### Application

Allowed:
- Domain
- Common
- application contracts/interfaces

Not allowed:
- API
- Infrastructure implementations
- UI host projects
- direct database/provider logic

### Infrastructure

Allowed:
- Domain
- Application contracts
- Common
- EF Core / provider packages / persistence details

Not allowed:
- API owning Infrastructure behavior
- UI host concerns
- transport/controller concerns

### Auth

Preferred:
- depend on Domain/Application/Common abstractions
- integrate with persistence through contracts

Avoid:
- becoming a second composition root
- forcing upper layers to reference Infrastructure directly

### API

Allowed:
- Application
- Domain only if needed for transport mapping or registration
- Infrastructure for composition root wiring only
- Auth for transport-facing auth integration

API should:
- define endpoints/controllers
- compose services
- define HTTP policy and middleware
- stay thin

API should not:
- become the only place where core service wiring exists
- own behavior needed by other executables such as migration tooling

### MigrationTool

Allowed:
- Infrastructure
- Auth/Application contracts if truly needed
- shared bootstrapping code in a neutral location

Not allowed:
- dependency on API-specific service registration

### Shared Client/UI

Allowed:
- Common
- client-side abstractions
- shared UI components that are actually cross-host

Avoid:
- packing host-specific behavior into shared components
- mixing UI concerns with every client/application service by default

## Practical Rules

### Rule 1: Composition Root Must Not Be Trapped in API

If service registration is needed by API and MigrationTool, move it into a neutral assembly/extension location rather than forcing MigrationTool to depend on API.

### Rule 2: Domain Must Stay Framework-Light

Do not add new infrastructure or web framework dependencies to Domain.

If a current framework dependency exists for historical reasons, treat it as technical debt, document it, and avoid spreading it further.

Do not mix runtime domain models and persisted entity models in the same abstraction just because they represent similar concepts. Keep the families distinct unless a temporary bridge is explicitly documented.

Keep domain rules, calculators, and engines pure when their role is deterministic output generation. Do not hide persistence, service lookups, or transport concerns inside them.

### Rule 3: Configuration Must Be Externalized

Do not hardcode:
- API base URLs
- CORS origins
- connection strings
- environment-specific toggles

Use configuration + options instead.

### Rule 4: One Clear Migration Story Per Environment

Each environment should have a clearly documented way to:
- apply schema changes
- seed data
- validate startup state

Avoid overlapping paths that all claim ownership.

### Rule 5: Shared Code Must Earn Its Place

A class belongs in shared code only if it is genuinely shared across hosts and not merely convenient to place there.

### Rule 6: Narrow Repositories Must Stay Entity-Local

Use-case-specific repository methods are acceptable when they stay within the ownership of that repository’s aggregate or entity and do not turn into a general read-model layer.

### Rule 7: Domain-Significant Literals Must Be Centralized

Repeated business identifiers such as posting source types, runtime rule scopes, fiscal status codes, and configuration keys should live in constants, enums, or options rather than being scattered as raw strings.

Review domain-significant numbers the same way. Centralize them only when they express shared business rules or precision standards, not for one-off local values.

### Rule 8: Abstractions Must Earn Their Keep

Introduce a new abstraction only when it removes a real coupling problem, isolates a volatile dependency, or eliminates meaningful duplication. Do not add layers “just in case.”

### Rule 9: Orchestration Must Stay Thin When Intended

If a service is meant to coordinate loading, validation, execution, persistence, and transactions, keep business rule selection and calculation logic out of it unless the design explicitly assigns that responsibility there.

## Known Current Pressure Points

These are current cleanup hotspots and should be treated as active refactor targets:

1. API-centric service registration that other executables rely on.
2. Migration tooling depending on API wiring.
3. Hardcoded environment/runtime values in client hosts and tests.
4. Migration + seeding responsibilities spread across multiple paths.
5. Shared project mixing UI, API client, auth/session, and view-model concerns.

## Refactor Priority Order

1. Composition root cleanup
2. Configuration externalization
3. Migration/seeding clarification
4. Test architecture cleanup
5. Shared client/UI boundary cleanup

## Change Review Checklist

Before approving a structural change, ask:

- Did this improve dependency direction?
- Did this reduce coupling or just move it?
- Did this preserve behavior?
- Did this create a clearer home for the code?
- Did this make config more external and less hardcoded?
- Did this reduce future maintenance cost?

If the answer is "no" to most of those, the change is probably not worth making.
