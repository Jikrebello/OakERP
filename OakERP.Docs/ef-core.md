## 🧰 EF Core CLI Helper (Windows PowerShell)

To simplify working with migrations and updating your local dev database, a helper script `ef.ps1` is included in the root of the solution.

This script assumes:
- Your **DbContext** lives in `OakERP.Infrastructure`
- Your **Startup project** is `OakERP.WebAPI`

---

### 📜 Commands & Usage

```powershell
# Add a new migration
.\ef.ps1 -action add -name AddTenantLicenseTable

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

# Rollback a broken or unnecessary migration
.\ef.ps1 -action rollback

# Rebuild your database from scratch with a clean migration
.\ef.ps1 -action drop
.\ef.ps1 -action add -name InitSchema
.\ef.ps1 -action update

# Full reset (drop + apply latest migrations)
.\ef.ps1 -action reset
```

---

### 🧼 When to Use This

| Task       | Use This When                                                                 |
|------------|-------------------------------------------------------------------------------|
| `add`      | You’ve changed your models and want to capture the difference as a migration. |
| `remove`   | You added a migration by mistake and want to remove it (before updating DB).  |
| `update`   | You want to apply the latest migrations to the database.                      |
| `drop`     | You want to nuke the DB during development and start over.                    |
| `rollback` | You need to undo the last migration, both in DB and source code.              |
| `reset`    | You want to fully reset the database and re-apply all migrations cleanly.     |
| `status`   | You want to see which migrations exist and are applied.                       |

---

### 🛑 Warnings

- This script is **only for local dev environments**.
- Do **not** use `drop` or `rollback` in production or on shared environments.
- If you're using Docker, ensure the `oakdb` service is running before using `update`, `reset`, or `rollback`.
