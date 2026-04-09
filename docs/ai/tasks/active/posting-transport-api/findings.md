# Findings

## Current State
- OakERP already has resource-specific document controllers for AP invoices, AP payments, AR invoices, and AR receipts.
- Existing controller style favors resource actions on the current controllers, as shown by `/{id}/allocations` on payments and receipts.
- Posting is already implemented behind `IPostingService`, `PostCommand`, and `PostResult`.
- `GlobalExceptionHandler` already maps `OakErpException` through `FailureKind`.
- `PostingInvariantViolationException` currently hard-codes `FailureKind.Unexpected`, which is why expected posting business rejections surface as `500`.
- `GlobalExceptionHandler` also needed to assign `HttpContext.Response.StatusCode` from the generated `ProblemDetails`; without that, exception responses stayed at the middleware default `500` even when `FailureKind` mapped to `400`, `404`, or `409`.

## Chosen Direction
- Keep posting transport resource-specific on the existing document controllers.
- Keep the failure normalization local to posting by extending `PostingInvariantViolationException` to carry an explicit `FailureKind`.
- Normalize only the three central posting-state checks in `PostingOperationSupport`:
  - non-draft or already-posted -> `409`
  - no open fiscal period -> `409`
  - non-base-currency -> `400`

## Constraints
- No API-layer message parsing.
- No generic posting controller.
- No posting-contract redesign.
- No unposting, bank transaction, or FX redesign work in this slice.

## Test Notes
- API integration tests should assert both status code and `ProblemDetails` title/type for failure routes.
- Direct posting integration tests remain the deeper coverage for posting internals; the new API tests focus on HTTP transport and middleware behavior.
