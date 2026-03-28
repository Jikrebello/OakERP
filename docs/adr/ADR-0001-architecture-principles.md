# ADR-0001: OakERP Architecture Principles

## Status
Accepted

## Context
OakERP is in an early but already structured state. The goal is to preserve the good existing separation while correcting dependency direction, reducing configuration hardcoding, and simplifying migration, testing, and runtime concerns.

The repo is also being prepared for agent-assisted refactoring, which means architectural intent must be written down clearly enough for both humans and tools to follow.

## Decision
OakERP will be refactored according to these principles:

1. Dependency direction matters more than folder cosmetics.
2. Behavior-preserving refactors are preferred over broad rewrites.
3. Environment-specific values belong in configuration, not compiled code.
4. Shared projects must have narrow, intentional responsibility.
5. Tooling and migration entry points should not depend on API-only composition code.
6. Large or risky refactors should be planned and documented in task files.
7. Validation is required after meaningful changes through build and relevant tests.

## Intended Layering
- Domain is the business core.
- Application contains use cases, contracts, and orchestration.
- Infrastructure implements persistence and external integrations.
- API is a transport/composition layer.
- Client hosts compose UI and client-side services.

## Immediate Cleanup Priorities
1. Composition root and dependency direction
2. Configuration externalization
3. Migration and seeding cleanup
4. Test architecture cleanup
5. Shared client/UI boundary cleanup

## Consequences
### Positive
- Refactors become more deliberate and less chaotic
- Codex and other agents get clearer boundaries
- Architectural drift is easier to detect
- Later modularization becomes easier

### Negative
- Slightly more documentation overhead
- Some existing shortcuts will need to be removed
- Early refactors may feel slower because they are more deliberate

## Notes
This ADR is the baseline architectural intent, not the final architecture design.
Future ADRs can refine specific topics such as module boundaries, testing strategy, migration flow, and observability.