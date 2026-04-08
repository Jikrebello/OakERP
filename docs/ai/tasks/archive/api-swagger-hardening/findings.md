# Findings

- `OakERP.API` currently registers only auth schema example filters.
- Current controller actions outside auth mostly have no `SwaggerOperation`, `Consumes`, `Produces`, or explicit non-success response metadata.
- Swagger is only enabled in `Development`, while the integration host runs in `Testing`.
- There are no source-level or integration Swagger document tests today.
- API result status mapping already exists through `ResultStatusCodeResolver`, so response documentation can align with the current transport behavior without changing business logic.
