# Progress

## Task
ar-invoice-backend

## Started
2026-04-08 23:37:46

## Work Log
- Created the task scaffold with `new-codex-task.ps1`.
- Confirmed the existing AR invoice aggregate and posting flow already support `RevenueAccount`, `ItemId`, `LocationId`, and `TaxRateId`.
- Confirmed the slice should include Swagger/example-filter parity and a mixed service-plus-item example payload.
- Added the missing AR invoice Application slice under `OakERP.Application/AccountsReceivable/Invoices`.
- Added the thin `POST /api/ar-invoices` controller plus Swagger metadata and a mixed-line request example filter.
- Added create-time validation and workflow handling for customer, currency, revenue account, item, location, tax rate, totals, and `DocNo` uniqueness.
- Added the narrow `ExistsDocNoAsync` repository helper and AR invoice persistence-conflict classification.
- Completed Infrastructure wiring for `IItemRepository`, `ILocationRepository`, and the previously missing `ITaxRateRepository` implementation.
- Added unit tests for validator, snapshot mapping, workflow/service behavior, and result-path conflict handling.
- Added integration tests for mixed-line draft creation, rollback behavior, and Swagger/example parity.
- Tightened AR invoice API test seeding to use per-test business keys and document numbers so the tests remain isolated in the shared integration database.
- Tightened the existing `ArInvoicePostingTests` seed helper so compatibility validation reaches posting behavior instead of failing on duplicate shared seed data.
- Broadened integration-test hardening to the adjacent AP payment, AP invoice, AR receipt, and auth API suites so repo-wide validation can run cleanly in the shared seeded test database.
- Fixed API/integration test collisions by switching reused auth tenant identities and AP/AR document seeds to per-test unique values and by shortening generated AP invoice numbers to stay within persisted column limits.
- Ran CSharpier over the touched integration files after the broader validation pass exposed formatting drift.

## Files Touched
- `OakERP.API/Controllers/ArInvoicesController.cs`
- `OakERP.API/Extensions/ServiceCollectionExtensions.cs`
- `OakERP.API/GlobalUsings.cs`
- `OakERP.API/Swagger/Examples/AccountsReceivable/CreateArInvoiceCommandExampleFilter.cs`
- `OakERP.Application/AccountsReceivable/Invoices/*`
- `OakERP.Application/Extensions/ServiceCollectionExtensions.cs`
- `OakERP.Application/GlobalUsings.cs`
- `OakERP.Application/Interfaces/Persistence/IPersistenceFailureClassifier.cs`
- `OakERP.Domain/RepositoryInterfaces/AccountsReceivable/IArInvoiceRepository.cs`
- `OakERP.Infrastructure/Extensions/ServiceCollectionExtensions.cs`
- `OakERP.Infrastructure/Persistence/PersistenceFailureClassifier.cs`
- `OakERP.Infrastructure/Repositories/AccountsReceivable/ArInvoiceRepository.cs`
- `OakERP.Infrastructure/Repositories/Common/TaxRateRepository.cs`
- `OakERP.Tests.Unit/AccountsReceivable/ArInvoice*`
- `OakERP.Tests.Unit/AccountsPayable/ApInvoiceServiceTestFactory.cs`
- `OakERP.Tests.Unit/AccountsPayable/ApPaymentServiceTestFactory.cs`
- `OakERP.Tests.Unit/AccountsReceivable/ArReceiptServiceTestFactory.cs`
- `OakERP.Tests.Unit/Posting/PostingServiceTestFactory.cs`
- `OakERP.Tests.Unit/GlobalUsings.cs`
- `OakERP.Tests.Integration/AccountsReceivable/ArInvoiceApiTests.cs`
- `OakERP.Tests.Integration/AccountsPayable/ApInvoiceApiTests.cs`
- `OakERP.Tests.Integration/AccountsPayable/ApPaymentApiTests.cs`
- `OakERP.Tests.Integration/AccountsReceivable/ArReceiptApiTests.cs`
- `OakERP.Tests.Integration/Auth/AuthApiTests.cs`
- `OakERP.Tests.Integration/Posting/ApInvoicePostingTests.cs`
- `OakERP.Tests.Integration/Posting/ApPaymentPostingTests.cs`
- `OakERP.Tests.Integration/Posting/ArInvoicePostingTests.cs`
- `OakERP.Tests.Integration/Posting/ArReceiptPostingTests.cs`
- `OakERP.Tests.Integration/Swagger/SwaggerDocumentTests.cs`
- `OakERP.Tests.Integration/TestSetup/ApiRoutes.cs`
- `OakERP.Tests.Integration/GlobalUsings.cs`
- `docs/ai/tasks/active/ar-invoice-backend/task_plan.md`
- `docs/ai/tasks/active/ar-invoice-backend/findings.md`
- `docs/ai/tasks/active/ar-invoice-backend/progress.md`

## Validation
- `dotnet build OakERP.API/OakERP.API.csproj` - passed
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj --filter FullyQualifiedName~ArInvoice` - passed
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ArInvoiceApiTests` - passed
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~SwaggerDocumentTests` - passed
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~ArInvoicePostingTests` - passed
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter "FullyQualifiedName~ApInvoiceApiTests|FullyQualifiedName~ApPaymentApiTests|FullyQualifiedName~ArReceiptApiTests"` - passed
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj --filter FullyQualifiedName~AuthApiTests` - passed
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj` - passed
- `dotnet build OakERP.sln` - passed
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj` - passed
- `pwsh ./tools/validate-pr.ps1` - passed

## Remaining
- No known remaining validation work for this slice pass.

## Deferred Smells / Risks
- No scope widening into posting redesign, UI, or bank transactions.
- No schema or migration work was introduced.
- The broader repo pass required narrow test-isolation hardening in existing AP/AR/auth integration tests because the current integration environment reuses shared seeded data across fixtures.

## Outcome
- The AR invoice backend create slice is implemented end to end with posting-compatible draft persistence and Swagger parity.
- Full solution build, unit tests, integration tests, and `validate-pr` all pass after the adjacent test-harness fixes.

## Next Recommended Step
- Stage the slice for review or continue with PR packaging.
