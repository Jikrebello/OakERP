# 🌳 OakERP

[![.NET](https://img.shields.io/badge/.NET%209+-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![MAUI](https://img.shields.io/badge/MAUI-Blazor-blueviolet?logo=microsoft&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/maui/)
[![PostgreSQL](https://img.shields.io/badge/Database-PostgreSQL-336791?logo=postgresql&logoColor=white)](https://www.postgresql.org/)

> Modern ERP workbench, built with deliberate architecture, shared UI, and a very real bias toward maintainability.

OakERP is a modular ERP platform being built around clean architecture, cross-host UI reuse, and explicit operational boundaries. The current solution combines an ASP.NET Core API, a Blazor web host, MAUI desktop/mobile hosts, a layered backend, and a growing test/documentation/tooling story.

---

## 🧱 Current Shape

The repo is organized around real projects rather than a generic `Apps/Core/Infrastructure` shell:

- `OakERP.API`: HTTP transport, runtime policy, Swagger, and composition root
- `OakERP.Application`: use cases, orchestration, settlements, and posting services
- `OakERP.Domain`: business entities, repository contracts, and posting runtime models
- `OakERP.Infrastructure`: EF Core, repositories, persistence, seeding, and posting data builders
- `OakERP.Auth`: auth workflows, JWT generation, identity seams, and registration/login coordination
- `OakERP.Client`, `OakERP.Shared`, `OakERP.UI`: client plumbing, shared UI shell, and UI-state concerns
- `OakERP.Web`, `OakERP.Desktop`, `OakERP.Mobile`: host projects
- `OakERP.Tests.Unit`, `OakERP.Tests.Integration`: unit and end-to-end runtime validation
- `OakERP.MigrationTool`: operational migration/seeding executable
- `OakERP.Docs` and `docs/architecture`: developer and architecture guidance

---

## 🚧 Current State

Implemented or materially in progress today:

- AP invoice capture and posting
- AP payment capture, allocation, and posting
- AR receipt capture, allocation, and posting
- auth register/login flow with JWT-based API access
- API Swagger/OpenAPI coverage for the current controller surface
- architecture cleanup across Application/Auth/API/Shared boundaries
- unit, integration, runtime, posting, and Swagger smoke test coverage

Still intentionally incomplete or early:

- broader AR invoice workflow surface
- deeper module coverage beyond the current AP/AR/posting/auth slices
- fuller Mobile functional alignment with Web/Desktop
- broader user-facing product docs

---

## 🧭 Where To Start

- Local development entrypoint: [OakERP.Docs/README-dev.md](OakERP.Docs/README-dev.md)
- Docker database setup: [OakERP.Docs/db-setup.md](OakERP.Docs/db-setup.md)
- Local HTTPS certificate fix: [OakERP.Docs/local-https-dev-cert.md](OakERP.Docs/local-https-dev-cert.md)
- EF Core migrations: [OakERP.Docs/ef-core.md](OakERP.Docs/ef-core.md)
- Testing guide: [OakERP.Docs/oakERP-testing-guide.md](OakERP.Docs/oakERP-testing-guide.md)
- API conventions: [OakERP.Docs/api-conventions.md](OakERP.Docs/api-conventions.md)
- Architecture overview: [OakERP.Docs/architecture-overview.md](OakERP.Docs/architecture-overview.md)
- Dependency rules: [docs/architecture/dependency-rules.md](docs/architecture/dependency-rules.md)
- Project map: [docs/architecture/project-map.md](docs/architecture/project-map.md)

---

## 🚀 Quick Local Flow

1. Start PostgreSQL and pgAdmin with Docker.
2. Create local config overrides for the API and Web hosts.
3. Trust the ASP.NET Core localhost development certificate.
4. Start the API on `https://localhost:7057` or `http://localhost:5169`.
5. Start the Web host on `https://localhost:7094` or `http://localhost:5067`.
6. Use `ef.ps1` for schema work and run unit/integration tests from the repo root.

---

## ✅ Validation Defaults

From the repo root:

```powershell
dotnet build OakERP.sln
dotnet test OakERP.Tests.Unit/OakERP.Tests.Unit.csproj
dotnet test OakERP.Tests.Integration/OakERP.Tests.Integration.csproj
```

For Codex-driven backend work, also see [docs/ai/codex-workflow.md](docs/ai/codex-workflow.md).
