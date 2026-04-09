# Task Plan

## Task Name
posting-transport-api

## Goal
Expose posting through resource-specific API endpoints and normalize the obvious expected posting business rejections away from `500` without redesigning posting internals.

## Scope
- `OakERP.API`
- `OakERP.Application`
- `OakERP.Common`
- `OakERP.Tests.Unit`
- `OakERP.Tests.Integration`
- `docs/ai/tasks/active/posting-transport-api`

## Out of Scope
- unposting transport
- bank transaction creation
- FX redesign
- posting-engine redesign
- frontend, web, desktop, or mobile work
- generic `/api/posting` controller

## Planned Steps
1. Add a shared posting request DTO, Swagger example registration, and `/{id}/post` actions on the four existing document controllers.
2. Add a narrow posting-local normalization seam by letting `PostingInvariantViolationException` carry a non-default `FailureKind` and updating only the three central checks in `PostingOperationSupport`.
3. Add focused unit tests, API integration tests, and Swagger coverage, then run targeted validation.

## Validation Commands
```powershell
dotnet build OakERP.API/OakERP.API.csproj
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter "FullyQualifiedName~PostingOperationSupportTests|FullyQualifiedName~DocumentPostingControllerTests"
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter "FullyQualifiedName~ApInvoiceApiTests|FullyQualifiedName~ApPaymentApiTests|FullyQualifiedName~ArInvoiceApiTests|FullyQualifiedName~ArReceiptApiTests|FullyQualifiedName~SwaggerDocumentTests"
```

## Success Criteria
- `POST /api/ap-invoices/{invoiceId}/post`
- `POST /api/ap-payments/{paymentId}/post`
- `POST /api/ar-invoices/{invoiceId}/post`
- `POST /api/ar-receipts/{receiptId}/post`
- Successful posts return `200 OK` with `PostResult`.
- Expected posting business rejections return normalized `4xx` `ProblemDetails` responses via the existing exception pipeline.
- Swagger documents the new routes, request schema/example, success schema, and error responses.
