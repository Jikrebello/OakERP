# OakERP API Conventions

This is the short guide to the current API conventions used by `OakERP.API`.

## Controller Pattern

Current API controllers are controller-based, not Minimal APIs.

Key conventions:

- controllers stay transport-thin
- application/auth services return result DTOs
- transport status mapping happens in the API layer
- unexpected and semantic exceptions flow through the global exception handler

Important types:

- `BaseApiController`
- `BaseResultDto`
- `ResultStatusCodeResolver`
- `GlobalExceptionHandler`

## Result DTO Transport Pattern

The current pattern is:

- `Success = true` -> API returns `200 OK`
- `Success = false` -> API maps the `FailureKind` to an HTTP status code and returns the same DTO body

Current `FailureKind` mapping:

- `Validation` -> `400`
- `NotFound` -> `404`
- `Conflict` -> `409`
- `Unauthorized` -> `401`
- `Forbidden` -> `403`
- `Unexpected` -> `500`

The API also sets `StatusCode` on failed result DTOs before returning them.

## Exception Handling

Exceptions that inherit from `OakErpException` are translated into `ProblemDetails` by the global exception handler.

Behavior:

- semantic OakERP exceptions map to the matching HTTP status code
- unexpected exceptions map to `500`
- development responses include more detail than non-development responses
- correlation and trace identifiers are included through the runtime support path

## Swagger / OpenAPI

The API uses Swashbuckle and currently documents:

- auth endpoints
- user probe endpoints
- AP invoice creation
- AP payment creation/allocation
- AR receipt creation/allocation

Current expectations:

- each action has explicit summaries/descriptions
- success and important failure statuses are documented
- write endpoints document `Consumes("application/json")`
- request examples exist for auth/AP/AR command payloads
- protected endpoints document bearer security

Swagger JSON is available in:

- `Development`
- `Testing`

Swagger UI remains development-only.

## Auth And Security Conventions

- bearer JWT auth is the current API auth mechanism
- protected endpoints use `[Authorize]`
- role-restricted probes use role-based authorization
- anonymous auth endpoints stay explicitly `[AllowAnonymous]`

## Runtime Support

API-specific runtime concerns currently owned by `OakERP.API` include:

- Swagger/OpenAPI registration
- exception handling and ProblemDetails
- CORS policy
- rate limiting
- request timeouts
- health endpoints
- request/correlation logging middleware

These are transport/runtime policies, not application-service responsibilities.
