# Progress

## Work Completed
- Added shared client error abstractions and logging fallback handler.
- Routed `ApiClient` transport and unexpected deserialization exceptions through the shared client error handler.
- Added toast-backed frontend error handling in `OakERP.UI`.
- Added reusable `UiOperationRunner` for busy-state cleanup and unexpected exception routing.
- Refactored login/register view models to use the operation runner.
- Added `OakErrorBoundary` and wired routes through it.
- Added targeted unit tests for client error handling, frontend notification routing, and busy-state reset.
- Updated auth view-model tests to verify invalid forms do not enter the operation runner.

## Files Touched
- `OakERP.Client`
- `OakERP.UI`
- `OakERP.Shared`
- `OakERP.Tests.Unit`
- `docs/ai/tasks/active/global-exception-handling-cleanup`

## Validation Results
- `dotnet build OakERP.Client/OakERP.Client.csproj` passed.
- `dotnet build OakERP.UI/OakERP.UI.csproj` passed.
- `dotnet build OakERP.Shared/OakERP.Shared.csproj` passed.
- `dotnet build OakERP.Web/OakERP.Web.csproj` passed.
- `dotnet build OakERP.Desktop/OakERP.Desktop.csproj` passed.
- `dotnet build OakERP.API/OakERP.API.csproj` passed.
- Targeted unit tests passed: `GlobalExceptionHandlerTests`, `AuthViewModelTests`, `ClientErrorHandlingTests`.
- Targeted integration tests passed: `RuntimeSupportTests`, `AuthApiTests`.
- `dotnet csharpier check .` passed.

## Remaining
- No implementation work remains for this slice.

## Deferred Smells / Risks
- Broader frontend feature view models may still need migration to `UiOperationRunner` as they are added or touched.
- The API result/exception distinction should remain explicit: expected business failures return DTOs; unexpected failures use global exception handling.
