Oak ERP Solution Structure

/OakERP.Solution
│
├── /OakERP.Desktop              <-- .NET MAUI Blazor desktop app
│   └── ViewModels/
│
├── /OakERP.Client              <-- .NET MAUI Blazor web server app
│   └── ViewModels/
│
├── /OakERP.Mobile               <-- .NET MAUI Blazor mobile app (POS, CRM, etc.)
│   └── ViewModels/
│   └── Views/
│
├── /OakERP.WebAPI               <-- ASP.NET Core WebAPI (online sync, endpoints)
│   └── Controllers/
│   └── HostedServices/         <-- Background sync, jobs
│   └── Middleware/
│   └── Installers/             <-- Modular DI setup
│
├── /OakERP.Application          <-- Business logic, interfaces, services
│   ├── Services/
│   ├── Interfaces/
│   ├── Reports/
│   ├── Dashboards/
│   └── Notifications/
│
├── /OakERP.Domain               <-- Core domain models and rules
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Enums/
│   ├── DomainEvents/
│   └── Interfaces/
│
├── /OakERP.Infrastructure       <-- EF Core, Repos, Templates, Auth, Background Tasks
│   ├── Data/
│   ├── Repositories/
│   ├── Migrations/
│   ├── PdfRendering/
│   ├── ReportTemplates/
│   ├── BackgroundJobs/
│   ├── NotificationProviders/
│   └── AuthProviders/
│
├── /OakERP.Reports              <-- Razor Class Library for printable/exportable reports
│   ├── Templates/
│   ├── Components/
│   ├── Styles/
│   └── wwwroot/
│
├── /OakERP.UI                   <-- Shared Blazor components, styles, icons
│   ├── Components/
│   ├── Themes/
│   ├── Icons/
│   └── wwwroot/
│
├── /OakERP.Auth                 <-- Token handling, roles, identity management
│   ├── AuthService.cs
│   ├── TokenStore.cs
│   ├── IAuthProvider.cs
│   └── Models/
│
├── /OakERP.Notifications        <-- Centralized notification and messaging system
│   ├── INotifier.cs
│   ├── NotificationService.cs
│   └── Models/
│
├── /OakERP.MigrationTool        <-- CLI tool for DB migrations and data import
│   ├── Program.cs
│   ├── /Importers/              <-- Excel, Sage, JSON
│   ├── /Schemas/                <-- Excel templates (downloadable)
│   ├── /Mappers/                <-- Data → DTO → Entities
│   ├── /Validations/            <-- Data integrity checks
│   └── /Seeds/                  <-- Optional: per-tenant or industry-based
│
├── /OakERP.Shared               <-- DTOs, constants, settings, mapping
│   ├── DTOs/
│   ├── Extensions/
│   ├── Mapping/
│   ├── Constants/
│   └── AppSettings/
│
├── /OakERP.Tests.Unit           <-- Unit tests (Domain + Application layer)
├── /OakERP.Tests.Integration    <-- Integration tests (API, ViewModels, EF)
│
├── /OakERP.Docs                 <-- Auto-generated and developer documentation
│   ├── SwaggerConfig/          <-- Swagger setup and extensions
│   ├── ApiContracts/           <-- Shared OpenAPI definitions or examples
│   ├── DeveloperGuides/        <-- Optional: Markdown or static docs
│   └── DocAssets/              <-- Logos, diagrams, or schema images
│
├── /docker                      <-- Docker configs and supporting services
│   ├── /postgres/               <-- Init scripts and volume config
│   │   ├── init/
│   │   └── data/
│   ├── /pgadmin/                <-- Optional DB GUI
│   ├── /mailhog/                <-- Fake SMTP for dev email
│   ├── /nginx/                  <-- (Optional) static hosting or reverse proxy
│   ├── /redis/                  <-- (Optional) Redis for caching/queueing
│   └── /seed/                   <-- Sample fixtures or JSON test data
│
├── docker-compose.yml           <-- Starts API, DB, MailHog, Redis, etc.
├── .env                          <-- Environment-specific config
├── README.md                     <-- High-level overview and tech stack
└── README-dev.md                 <-- Dev-specific: setup, Docker, migrations, tests
