# 🧰 EF Core CLI Helper (PowerShell)

This script (`ef.ps1`) lives at the **solution root** and makes EF Core tasks simple and repeatable. It can:

* **Auto-detect** your DbContext project and Startup project
* **Generate / apply / roll back** migrations
* **Create SQL scripts** (idempotent or targeted)
* **Override the connection string** (great for Docker/dev/CI)
* Create **ignore-changes** migrations (handy to pre-create PostgreSQL enums)

Works on Windows PowerShell 7+.

---

## 🧠 Mental model

EF migrations need two things:

1. A **DbContext project** (where your `ApplicationDbContext` and entity configs live).
2. A **Startup project** (where EF can read runtime services/config, e.g., `appsettings.json`, DI, logging, etc.).

You can run migrations in two ways:

* **Through Startup** (recommended): EF boots your Startup assembly and uses whatever configuration it provides (connection string, DI, logging).
* **Direct connection override**: You pass a full `-connection "Host=...;Port=...;"` to the script and bypass Startup’s connection settings (useful for Docker port-maps, CI, or quick local overrides).

---

## ⚙️ Auto-detection

By default, the script will pick:

* **DbContext project**: first match of `*.Infrastructure.csproj`.
* **Startup project**: first match of `*.API.csproj`.
* **DbContext type**: `ApplicationDbContext`.

You’ll see what it picked at the top:

```
🧭 Using:
  Project:  <path>\YourApp.Infrastructure.csproj
  Startup:  <path>\YourApp.API.csproj
  Context:  ApplicationDbContext
```

> You can override any of these with `-project`, `-startup`, or `-context`.

---

## 🚀 First-time developer setup

1. **Install EF tools (once):**

   ```powershell
   dotnet tool install -g dotnet-ef
   ```

   Keep them current:

   ```powershell
   dotnet tool update -g dotnet-ef --version 9.0.10
   ```

2. **Start Postgres** (Docker example):

   * Container name: `oakdb`
   * Mapped port: `5433` (so it doesn’t clash with a local install)
   * User/pass/db: `oakadmin` / `oakpass` / `oakerp`

3. **Verify connection (optional but handy):**

   ```powershell
   docker exec -it oakdb psql "host=localhost port=5433 dbname=oakerp user=oakadmin password=oakpass" -c "select current_user, current_database();"
   ```

4. **Choose a connection strategy** (see “Two ways to run migrations” below) and you’re ready.

---

## 📜 Commands you’ll use

```powershell
# Add a migration (detects projects/context automatically)
.\ef.ps1 add Add_SomeFeature

# Apply latest migrations to the database
.\ef.ps1 update

# Remove the last (code-only) migration
.\ef.ps1 remove

# Roll back the last migration (DB revert + remove code)
.\ef.ps1 rollback

# Drop DB and apply all migrations again
.\ef.ps1 reset

# List migrations (applied + available)
.\ef.ps1 status

# Generate SQL (optionally idempotent)
.\ef.ps1 script -from 0 -to Latest -idempotent > .\deploy.sql
```

Shorthand (positional args) works too:

```powershell
.\ef.ps1 add Init_Schema
.\ef.ps1 update
```

---

## 🔧 Useful switches

| Switch / Param   | Purpose                                                                        | Example                                                                                     |
| ---------------- | ------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------- |
| `-project`       | Force the project that **contains** the DbContext                              | `-project .\MyApp.Infrastructure\MyApp.Infrastructure.csproj`                               |
| `-startup`       | Force the **Startup** project                                                  | `-startup .\MyApp.API\MyApp.API.csproj`                                                     |
| `-context`       | Use a **different DbContext** type                                             | `-context ReportingDbContext`                                                               |
| `-connection`    | Override the **connection string**                                             | `-connection "Host=localhost;Port=5433;Database=oakerp;Username=oakadmin;Password=oakpass"` |
| `-ignoreChanges` | For `add`: migration **without** model diffs (great for pre-creating PG enums) | `.\ef.ps1 add 0001_CreatePgEnums -ignoreChanges`                                            |
| `-to`            | Target a specific migration for `update`/`script`                              | `.\ef.ps1 update -to 20250101010101_Init`                                                   |
| `-from`          | Start migration in `script`                                                    | `.\ef.ps1 script -from 0 -to Latest`                                                        |
| `-idempotent`    | Generate idempotent SQL in `script`                                            | `.\ef.ps1 script -from 0 -to Latest -idempotent`                                            |
| `-noBuild`       | Skip building for faster runs                                                  | `-noBuild`                                                                                  |
| `-verbose`       | Pass `-v` to EF for extra logging                                              | `-verbose`                                                                                  |

> The script **masks passwords** when echoing `-connection`.

---

## 🛣️ Two ways to run migrations

### A) **Through Startup** (preferred)

Use this when your Startup app (API/Web/Host) contains your canonical connection string and DI setup (e.g., reads `appsettings.json`, user-secrets, environment variables, etc.).

**How to use:**

```powershell
# No connection string needed—Startup provides it
.\ef.ps1 add Init_GL_AP_AR_Inventory
.\ef.ps1 update
```

**When to choose:**

* Your Startup app already configures the DbContext connection
* You want migrations to reflect the same environment settings used at runtime
* You want DI/logging from Startup during design-time

> Make sure your **DesignTimeDbContextFactory** or your Startup path supports design-time creation (which you already have), and that the Startup project is resolvable by the script (or pass `-startup`).

### B) **Direct connection override** (fast local/dev)

Use this when you want to bypass Startup’s config and point straight to a DB (common with Docker mapped ports or CI pipelines).

**How to use:**

```powershell
.\ef.ps1 update -connection "Host=localhost;Port=5433;Database=oakerp;Username=oakadmin;Password=oakpass;Include Error Detail=true"
```

**When to choose:**

* Docker maps Postgres to a custom port (e.g., 5433)
* You want to run migrations against a specific database independently of app config
* CI/CD where the connection is injected securely as a variable/secret

---

## 🧪 Examples

### Pre-create PostgreSQL enums (robust pattern)

```powershell
# 1) Empty migration (no diffs) that only registers PG enums
.\ef.ps1 add 0001_CreatePgEnums -ignoreChanges

# 2) Real schema migration next
.\ef.ps1 add 0002_InitSchema

# 3) Apply
.\ef.ps1 update -connection "Host=localhost;Port=5433;Database=oakerp;Username=oakadmin;Password=oakpass"
```

### Generate a deploy script for ops/DBA

```powershell
# From baseline (0) to latest, idempotent
.\ef.ps1 script -from 0 -to Latest -idempotent > .\deploy.sql
```

### Force different projects/context

```powershell
.\ef.ps1 update `
  -project .\YourApp.Persistence\YourApp.Persistence.csproj `
  -startup .\YourApp.API\YourApp.API.csproj `
  -context ApplicationDbContext `
  -connection "Host=localhost;Port=5433;Database=oakerp;Username=oakadmin;Password=oakpass"
```

---

## 🧭 Connection strategy (pick one)

The script checks **in this order**:

1. `-connection` (explicit override)
2. `ConnectionStrings:DefaultConnection` in **DbContext project** `appsettings*.json`
3. `ConnectionStrings__DefaultConnection` environment variable
4. Hard-coded fallback inside the `DesignTimeDbContextFactory` (if present)

Example `appsettings.Development.json` beside your DbContext project:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5433;Database=oakerp;Username=oakadmin;Password=oakpass;Include Error Detail=true"
  }
}
```

---

## 🧯 Troubleshooting

**“password authentication failed for user …”**

* Confirm host/port/username/password. Quick check:

  ```powershell
  docker exec -it oakdb psql "host=localhost port=5433 dbname=oakerp user=oakadmin password=oakpass" -c "select current_user, current_database();"
  ```
* If you also have a local Postgres service, make sure your **Docker port** (e.g., 5433) is the one you target.

**“type ‘gl_account_type’ does not exist” (or any enum)**

* Either pre-create enums with an `-ignoreChanges` migration, **or** ensure enum registration runs before any tables that use them.

**EF tools/runtime mismatch warning**

* Keep the CLI tools aligned with runtime:

  ```powershell
  dotnet tool update -g dotnet-ef --version 9.0.10
  ```

---

## ✅ Requirements / Assumptions

* .NET **9** + EF Core **9**
* `Npgsql.EntityFrameworkCore.PostgreSQL` provider
* Solution contains:

  * An **Infrastructure/Persistence** project (DbContext + entity configs)
  * An **API/Web/Host** project (Startup)
* Default DbContext type: **ApplicationDbContext** (override with `-context`)
