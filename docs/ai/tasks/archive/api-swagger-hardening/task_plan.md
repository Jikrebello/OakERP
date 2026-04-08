# API Swagger Hardening

## Scope

- Limit changes to `OakERP.API` and API-focused integration tests.
- Add explicit Swagger/OpenAPI metadata for every current controller action.
- Expand schema example coverage beyond auth DTOs.
- Add Swagger JSON smoke tests through the existing integration test host.

## Constraints

- Preserve routes, DTO wire shapes, and controller behavior.
- Keep health endpoints out of Swagger scope for this task.
- Keep Swagger UI development-only.
- Make Swagger JSON reachable in `Testing` so integration tests can validate it.

## Ordered Steps

1. Reorganize Swagger example filters into feature-based folders and add missing examples.
2. Tighten `AddSwaggerDocs()` so annotations and all schema filters are registered centrally.
3. Add action-level OpenAPI metadata to Auth, Users, AP invoice, AP payment, and AR receipt controllers.
4. Expose `UseSwagger()` in `Testing`.
5. Add integration tests for `/swagger/v1/swagger.json` route coverage, security metadata, request body docs, and examples.
6. Run targeted API build, targeted Swagger tests, full integration tests, and solution build.

## Validation

- `dotnet build OakERP.API/OakERP.API.csproj`
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter Swagger`
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj`
- `dotnet build OakERP.sln`
