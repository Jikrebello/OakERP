# Global Exception Handling Cleanup

## Goal
Consolidate frontend/client exception handling while preserving the existing backend API global exception handler.

## Scope
- Keep the API `GlobalExceptionHandler` as the only backend exception boundary.
- Add shared client/frontend error handling for unexpected client exceptions.
- Replace repeated auth view-model busy-state `try/finally` blocks with a reusable UI operation runner.
- Keep expected business failures as explicit `ApiResult<T>` / DTO responses.

## Out Of Scope
- Replacing API business-result DTO handling with exceptions.
- Adding a second API exception middleware.
- Changing schema, migrations, Docker, or auth token behavior.
- Mobile-specific behavior beyond build compatibility through shared projects.

## Planned Steps
1. Add a client-side error handler abstraction in `OakERP.Client`.
2. Add toast-backed frontend handling and a reusable busy operation runner in `OakERP.UI`.
3. Replace the shared route `ErrorBoundary` with an OakERP boundary that routes render exceptions through the client error handler.
4. Refactor auth view models to use the operation runner.
5. Add targeted unit tests and run build/integration validation.

## Validation
```powershell
dotnet csharpier check .
dotnet build OakERP.API/OakERP.API.csproj
dotnet build OakERP.Client/OakERP.Client.csproj
dotnet build OakERP.UI/OakERP.UI.csproj
dotnet build OakERP.Shared/OakERP.Shared.csproj
dotnet build OakERP.Web/OakERP.Web.csproj
dotnet build OakERP.Desktop/OakERP.Desktop.csproj
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter "FullyQualifiedName~GlobalExceptionHandlerTests|FullyQualifiedName~AuthViewModelTests|FullyQualifiedName~ClientErrorHandlingTests"
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter "FullyQualifiedName~RuntimeSupportTests|FullyQualifiedName~AuthApiTests"
```

## Success Criteria
- API global exception behavior remains covered.
- Client transport/render/unexpected UI exceptions flow through a shared handler.
- Auth view models no longer contain repeated busy-state `try/finally` blocks.
- Expected auth/business failures still show specific messages.
- Targeted builds and tests pass.
