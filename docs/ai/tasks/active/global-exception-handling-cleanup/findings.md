# Findings

## Current State
- The backend API already had a global exception handler through `GlobalExceptionHandler`, `AddExceptionHandler`, `AddProblemDetails`, and `UseExceptionHandler`.
- API runtime tests already covered unhandled exception `ProblemDetails`, correlation IDs, trace IDs, and auth business-failure DTO preservation.
- The frontend had a Blazor `ErrorBoundary`, but it used a passive `ErrorToast` component and did not route the exception through a reusable service.
- `ApiClient` repeated method-level catch blocks for transport failures.
- Login/register view models repeated `try/finally` solely to reset `IsBusy`.

## Dependency Observations
- Non-UI client error abstractions belong in `OakERP.Client`.
- Toast-backed frontend behavior belongs in `OakERP.UI` because it depends on Fluent UI services.
- The Razor error boundary belongs in `OakERP.Shared`.
- No backend dependency-direction changes were needed.

## Implementation Notes
- Introduced `IClientErrorHandler` and supporting context/result records in `OakERP.Client`.
- Added a logging-only fallback handler so client services work outside UI hosts.
- Added a toast-backed frontend handler in `OakERP.UI` that overrides the fallback when shared host services are registered.
- Added `UiOperationRunner` to centralize busy-state cleanup and unexpected exception handling.
- Added `OakErrorBoundary` in `OakERP.Shared` to route render exceptions through the same frontend error handler.

## Risks / Deferred Items
- The remaining catch blocks are intentional centralized boundaries:
  - `ApiClient.SendAsync`
  - `ApiClient.HandleResponse` JSON fallback
  - `UiOperationRunner.RunBusyAsync`
- This does not convert expected business failures into exceptions.
- Mobile-specific runtime behavior was not expanded; shared project builds cover compatibility.

## Rollback / Transaction Notes
- Migration rollback reviewed: not applicable.
- Transactional failure leaves no writes: not applicable.
