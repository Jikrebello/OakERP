# Task Plan

## Scope

Implement waves 12 through 14 to reduce the last major maintainability gaps:
- shared AP/AR settlement-document orchestration policy
- shared MAUI host adapter source for Desktop and Mobile
- shared backend orchestration support for repeated transaction and failure plumbing

## Constraints

- preserve external behavior
- keep service interfaces unchanged
- avoid schema, route, DTO, and token-format changes
- keep Web host behavior intentionally separate from MAUI hosts where runtime differences are real

## Ordered Steps

1. Add shared settlement-document orchestration support and refactor AP payment and AR receipt create/allocation workflows onto it.
2. Move duplicate MAUI token/platform/form-factor adapters into shared MAUI host-core source and update Desktop/Mobile composition.
3. Add narrow orchestration helpers for repeated transaction and persistence-failure plumbing in Application/Auth and thin remaining hotspots.
4. Add or update unit and host-wiring tests for the new seams.
5. Run targeted validation first, then full solution build and test validation.

## Validation Plan

- `dotnet build OakERP.Application/OakERP.Application.csproj /nr:false /m:1`
- `dotnet build OakERP.Auth/OakERP.Auth.csproj /nr:false /m:1`
- `dotnet build OakERP.Shared/OakERP.Shared.csproj /nr:false /m:1`
- `dotnet build OakERP.Desktop/OakERP.Desktop.csproj /nr:false /m:1`
- `dotnet build OakERP.Mobile/OakERP.Mobile.csproj /nr:false /m:1`
- `dotnet build OakERP.Web/OakERP.Web.csproj /nr:false /m:1`
- `dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj /nr:false /m:1`
- `dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj /nr:false /m:1`
- `dotnet build OakERP.sln /nr:false /m:1`
