## 🧰 EF Core CLI Helper (Windows PowerShell)

This PowerShell helper script `ef.ps1` lives at the root of the solution and simplifies working with EF Core migrations. It **automatically detects** your `*.Infrastructure.csproj` and `*.API.csproj` files based on naming conventions, so you don’t need to specify project paths every time.

It supports common EF Core tasks like adding migrations, updating the database, rolling back changes, and more.

---

### ⚙️ How It Works

By default, the script:

- Detects the first `*.Infrastructure.csproj` (as the **DbContext project**)
- Detects the first `*.API.csproj` (as the **startup project**)
- Uses `AppDbContext` as the default context (can be overridden)

---

### 📜 Commands & Usage

```powershell
# Add a new migration
.\ef.ps1 -action add -name MigrationName

# Apply migrations to the database
.\ef.ps1 -action update

# Remove the last migration (code only, does NOT touch the database)
.\ef.ps1 -action remove

# Drop the entire database (careful!)
.\ef.ps1 -action drop

# Rollback the last migration (reverts DB and deletes the migration file)
.\ef.ps1 -action rollback

# Drop and immediately re-apply all migrations
.\ef.ps1 -action reset

# View migration history (applied + available)
.\ef.ps1 -action status
```

---

### 🧪 Real-World Examples

```powershell
# Add a migration after modifying ApplicationUser
.\ef.ps1 -action add -name AddUserTenantRelation

# Remove the last added migration (undo AddUserTenantRelation)
.\ef.ps1 -action remove

# Apply the latest migrations to your local PostgreSQL database
.\ef.ps1 -action update

# Rollback a broken or unnecessary migration
.\ef.ps1 -action rollback

# Drop the current database completely
.\ef.ps1 -action drop

# Rebuild your database from scratch with a clean migration
.\ef.ps1 -action drop
.\ef.ps1 -action add -name InitSchema
.\ef.ps1 -action update

# Full reset (drop + reapply all migrations from current model)
.\ef.ps1 -action reset

# View migration status (see applied and pending migrations)
.\ef.ps1 -action status
```

#### 💡 Tip: Positional Parameters Work Too

You can omit `-action` and `-name` if you prefer a shorter syntax. The script supports **positional arguments**, meaning these are equivalent:

```powershell
# Explicit form
.\ef.ps1 -action add -name AddUserTenantRelation

# Shorthand positional form (works the same)
.\ef.ps1 add AddUserTenantRelation
```

This works because:
- The first parameter is treated as `-action`
- The second (if present) is treated as `-name`

---

### 🧼 When to Use Each Command

| Command     | Purpose                                                                 |
|-------------|-------------------------------------------------------------------------|
| `add`       | You've changed your entities and want to create a new migration file.   |
| `remove`    | You accidentally added a migration and want to delete it.               |
| `update`    | Apply all pending migrations to your local dev DB.                      |
| `drop`      | DANGER: Nuke your local DB for a fresh rebuild.                         |
| `rollback`  | Undo the last applied migration (code + DB rollback).                   |
| `reset`     | Drop the DB, then reapply all migrations cleanly.                       |
| `status`    | View current and pending migrations.                                    |

---

### 🚧 Warnings & Notes

- This script is designed for **local development only**.
- Never run `drop`, `rollback`, or `reset` on a shared or production environment.
- Ensure Docker is running and your PostgreSQL container is healthy before using commands like `update`, `reset`, or `rollback`.

---

### ✅ Project Requirements

This script assumes:

- You are using **EF Core 9+**
- You are targeting a PostgreSQL database
- You are using a folder structure that includes:
  - `MyApp.Infrastructure` (with your `DbContext`)
  - `MyApp.API` (your startup project)
