## Current State

- `OakERP.Shared` currently contains shared Razor UI, routes, client API plumbing, auth/session plumbing, and auth view models.
- `OakERP.Web` and `OakERP.Desktop` both compose `AuthTokenHandler` and `IApiClient` directly against types that live in `OakERP.Shared`.
- `OakERP.Mobile` is not using the same shared client stack and should remain out of scope for this slice.
- There is no existing active `OakERP.Client` project in the solution, but an `OakERP.Client` folder already exists and can host a very small client-plumbing project without forcing a broader layout redesign.

## Dependency Interpretation

- Moving non-UI client plumbing into a small `OakERP.Client` project is feasible without moving shared view models, as long as `OakERP.Shared` references that project and keeps its UI-facing registration method.
- Preserving current namespaces for the moved plumbing types keeps host and UI changes minimal and avoids unnecessary rename churn.
- `AuthRoutes` are referenced by shared UI layout logic, but they are also required by the moved client auth service. To avoid a `Client -> Shared` reference cycle, they must move with the client plumbing while preserving their current namespace and values.

## Risks

- Creating a new small client project is acceptable here only if the move stays limited to non-UI plumbing.
- The main technical risk is accidentally widening into view-model, route, or host-composition redesign.
- Solution/build wiring must remain clean enough that Web and Desktop continue resolving `IApiClient` and auth/session services without behavior changes.
