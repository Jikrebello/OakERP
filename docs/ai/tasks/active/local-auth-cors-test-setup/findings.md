# Local Auth, CORS, And Test Setup Findings

## Current State

- `OakERP.Client.Routes.AuthRoutes` is used for both UI page navigation and API calls.
- The API exposes auth endpoints under `/api/auth/*`, but the client auth service posts to `Auth/*`.
- Login/register view models set `IsBusy` after a failed validation check and still call the API.
- `OakERP.API/appsettings.Local.json` is ignored by git but copied into test output and can override API host settings before services are registered.
- Integration reset config still points at `oakerp_test`, which can diverge from the database migrated by the API host if local config is loaded.
- API development config has no tracked Web CORS origins.
- Full integration validation no longer fails in setup after the local-config fix, but exposed posting API tests missing explicit posting prerequisites.

## Risks

- The ignored local API config contains local-only development settings and must not be committed.
- Desktop native HTTP is not CORS-bound; adding Desktop URLs as CORS origins would be misleading.
- Integration tests depend on local PostgreSQL availability on `localhost:5433`.

## Deferred

- Broader client route organization beyond auth remains out of scope.
- Mobile configuration remains out of scope.
