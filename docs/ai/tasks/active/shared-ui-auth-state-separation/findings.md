## Current State

- `OakERP.Shared` still contains the auth form models, auth view models, and their DI registration.
- The auth Razor pages in `OakERP.Shared` inject `LoginViewModel` and `RegisterViewModel` through the `OakERP.Shared.ViewModels.Auth` namespace.
- Moving those view models into another assembly while reusing `BaseViewModel` or `BaseFormViewModel` from `OakERP.Shared` would create a `Shared -> UI -> Shared` project-reference cycle.

## Dependency Interpretation

- The narrow move is still feasible if `OakERP.UI` owns only the auth form models, auth view models, and auth UI-state registration.
- To avoid a project cycle, the moved auth view models must carry their own tiny `Form`/`EditContext`/`IsBusy` state instead of inheriting `BaseFormViewModel`.
- Preserving the current namespaces for moved auth models and view models avoids Razor page churn.
- `OakERP.UI` must use package references for Blazor UI types instead of a `Microsoft.AspNetCore.App` framework reference, otherwise the Desktop MAUI targets fail on Mac Catalyst runtime-pack resolution.

## Risks

- `LoginViewModel` and `RegisterViewModel` must become publicly accessible once they move to a separate assembly so the existing Razor pages can inject them.
- `RegisterFormModel` must become public because it is exposed through the public `Form` property on the moved public view model.
- This slice should not pull generic base view-model classes or any shared layout/page code into `OakERP.UI`.
