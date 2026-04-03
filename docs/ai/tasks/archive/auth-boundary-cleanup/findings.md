## Current State

- `OakERP.Auth` references `OakERP.Infrastructure` only through `OakERP.Auth.csproj` and `OakERP.Auth/Extensions/ServiceCollectionExtensions.cs`.
- The concrete coupling is `AddIdentityServices()`, which registers `AddEntityFrameworkStores<ApplicationDbContext>()` and therefore imports `OakERP.Infrastructure.Persistence.ApplicationDbContext`.
- `AddIdentityServices()` is used by `OakERP.API/Program.cs` and `OakERP.MigrationTool/Main.cs`.
- `AuthService`, `IAuthService`, `IJwtGenerator`, and `ApplicationUser` are all still part of the current runtime behavior and must remain unchanged in this slice.

## Dependency Interpretation

- Moving only Identity store registration out of `OakERP.Auth` can reduce the concrete `Auth -> Infrastructure` dependency without touching schema or `ApplicationUser`.
- `OakERP.Infrastructure` already references `OakERP.Domain` and already depends on Identity EF packages, so it can own the `ApplicationDbContext`-backed Identity registration without creating a new project cycle.
- Once the transitive `Infrastructure` reference is removed, `OakERP.Auth` still needs a direct reference to `OakERP.Application` because `AuthService` depends on `IUnitOfWork`.
- Once `Infrastructure` owns Identity DI registration, it also needs access to the ASP.NET Core shared framework extensions that provide `AddIdentity(...)`.

## Risks

- Accidentally changing Identity options would affect login/register behavior.
- Accidentally widening the slice into contract or model changes would violate scope.
- Validation must stay sequential to avoid confusing file-lock noise with real regressions.
