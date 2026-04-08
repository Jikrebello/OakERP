# EF Core Migration Helper

OakERP keeps the `ef.ps1` helper at the repo root to make migration and schema operations repeatable.

It is designed around the current solution layout:

- DbContext project: `OakERP.Infrastructure`
- startup project: `OakERP.API`
- DbContext type: `ApplicationDbContext`

## First-Time Setup

Install or update the EF CLI tool:

```powershell
dotnet tool install -g dotnet-ef
dotnet tool update -g dotnet-ef --version 9.0.10
```

Start the local database first:

```powershell
docker compose up -d
```

Default local Postgres values:

- Host: `localhost`
- Port: `5433`
- Database: `oakerp`
- Username: `oakadmin`
- Password: `oakpass`

## Common Commands

```powershell
.\ef.ps1 add Add_SomeFeature
.\ef.ps1 update
.\ef.ps1 remove
.\ef.ps1 rollback
.\ef.ps1 reset
.\ef.ps1 status
.\ef.ps1 script -from 0 -to Latest -idempotent > .\deploy.sql
```

## Connection Strategy

The helper resolves the connection in this order:

1. explicit `-connection`
2. current host/app configuration through the startup project
3. design-time resolution in `OakERP.Infrastructure`

For most local work, the cleanest approach is:

- keep your local runtime settings in `OakERP.API/appsettings.Local.json`
- use `.\ef.ps1 update` when startup-based configuration is enough

For explicit targeting, use `-connection`:

```powershell
.\ef.ps1 update -connection "Host=localhost;Port=5433;Database=oakerp;Username=oakadmin;Password=oakpass;Include Error Detail=true"
```

## Useful Switches

- `-project`: override the DbContext project
- `-startup`: override the startup project
- `-context`: override the DbContext type
- `-connection`: use an explicit connection string
- `-ignoreChanges`: create an empty migration
- `-to`, `-from`, `-idempotent`: targeted update/script generation
- `-noBuild`, `-verbose`: execution helpers

## Typical OakERP Usage

### Add a migration

```powershell
.\ef.ps1 add Add_NewPostingRuleSupport
```

### Apply the latest migration

```powershell
.\ef.ps1 update
```

### Apply with an explicit local connection

```powershell
.\ef.ps1 update -connection "Host=localhost;Port=5433;Database=oakerp;Username=oakadmin;Password=oakpass;Include Error Detail=true"
```

### Generate an idempotent deploy script

```powershell
.\ef.ps1 script -from 0 -to Latest -idempotent > .\deploy.sql
```

## Notes For This Repo

- `OakERP.MigrationTool` exists for operational migration/seeding flows, but `ef.ps1` remains the quickest local schema helper.
- `Down()` methods should only reverse the matching migration’s `Up()`.
- If a migration or seeding task changes behavior, record it in the task docs under `docs/ai/tasks/active/...`.

## Troubleshooting

### Password or connection failures

Confirm Docker is running and that you are using host port `5433` from the machine.

### Enum/type-order migration failures

Use an `-ignoreChanges` migration when you need a schema step that should not yet reflect model diffs.

### Tool/runtime mismatch

Keep `dotnet-ef` aligned with the repo runtime:

```powershell
dotnet tool update -g dotnet-ef --version 9.0.10
```
