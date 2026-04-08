# Findings

## Task
refactor-waves-8-11

## Current State
Before this wave:
- `ResultError`, `BaseResultDto`, and `OakErpException` still carried transport-oriented HTTP status semantics
- `AuthService`, `ApPaymentService`, `ArReceiptService`, and `ApInvoiceService` still owned most of their orchestration bodies directly
- Web and Desktop used similar but duplicate host bootstrap code, while Mobile still used the MAUI template bootstrap path
- posting/runtime tests were concentrated in a few large files

## Relevant Projects
- `OakERP.Common`
- `OakERP.Application`
- `OakERP.Auth`
- `OakERP.API`
- `OakERP.Shared`
- `OakERP.Web`
- `OakERP.Desktop`
- `OakERP.Mobile`
- `OakERP.Tests.Unit`
- `OakERP.Tests.Integration`

## Dependency Observations
- API should own HTTP mapping, not Application/Auth/Common DTO and exception types.
- Auth can depend on the shared clock seam from `OakERP.Common.Time` without widening infrastructure coupling.
- Shared host bootstrap belongs in a layer already referenced by all three hosts; `OakERP.Shared` is the narrowest workable home here.

## Structural Problems
- App/Auth failure types exposed transport semantics directly.
- Auth/AP/AR services were still mixed-responsibility entrypoint plus workflow implementations.
- Mobile lagged behind the client host composition path already used by Web/Desktop.
- Large posting/runtime test files were hard to scan and easy to break during refactors.

## Literal / Model-Family Notes
- Repeated business-significant literals:
- `HttpStatusCode` usage had become the repeated transport literal family in non-transport layers.
- Repeated domain-significant numbers:
- Runtime-vs-persisted model-family conflicts:
- Thin orchestrators getting too thick:
- `AuthService`, `ApPaymentService`, `ArReceiptService`, and `ApInvoiceService` were still thicker than intended.
- Pure engines/calculators with side effects or lookups:

## Configuration / Environment Notes
- Web/Desktop already consumed `Api:BaseUrl` through validated options, but Mobile had not been aligned to that same configuration path.

## Testing Notes
- Unit tests asserting `StatusCode` directly on application result DTOs needed to move to `FailureKind` assertions.
- Integration/runtime helper types had to stay in one place when splitting runtime test files.

## Rollback / Transaction Notes
- Migration rollback reviewed:
- Transactional failure leaves no writes:
  existing AP/AR workflow rollback behavior was preserved and validated by the existing unit and integration suites.

## Open Questions
- None blocked implementation once the runtime/test file split was repaired.

## Deferred Smells / Risks
- AP/AR create workflows are thinner now, but they are still parallel family implementations rather than a deeper unified use-case model.
- The shared host bootstrap unifies startup composition, but host-local adapter duplication still exists by design.

## Recommendation
The next safe step is a focused cleanup of remaining parallel AP/AR workflow duplication or a deeper client host adapter consolidation, but only if there is a concrete maintenance payoff.

