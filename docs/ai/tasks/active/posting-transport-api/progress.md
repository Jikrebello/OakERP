# Progress

## Task
posting-transport-api

## Work Log
- Added a shared posting request DTO and Swagger example filter for post-route request bodies.
- Extended the existing AP/AR invoice and payment/receipt controllers with resource-specific `/{id}/post` actions.
- Added a narrow posting-local normalization seam by letting `PostingInvariantViolationException` carry a non-default `FailureKind`.
- Updated the three central expected-business checks in `PostingOperationSupport` to normalize non-draft, no-open-period, and non-base-currency failures.
- Fixed the existing exception transport path by setting `HttpContext.Response.StatusCode` inside `GlobalExceptionHandler` so normalized `FailureKind` values flow through to HTTP responses instead of defaulting to `500`.
- Added focused unit tests for the normalization seam and controller-to-`PostCommand` mapping.
- Added API integration tests for the new post routes, including `ProblemDetails` assertions for stable title/type metadata.
- Extended Swagger integration coverage for the new routes and request/response schemas.

## Validation
- `dotnet build OakERP.API/OakERP.API.csproj` - passed
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter "FullyQualifiedName~PostingOperationSupportTests|FullyQualifiedName~DocumentPostingControllerTests"` - passed
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter "FullyQualifiedName~ApInvoiceApiTests|FullyQualifiedName~ApPaymentApiTests|FullyQualifiedName~ArInvoiceApiTests|FullyQualifiedName~ArReceiptApiTests|FullyQualifiedName~SwaggerDocumentTests"` - passed

## Deferred
- Unposting transport
- Bank transaction creation
- FX redesign
- Posting-engine redesign
- UI work
