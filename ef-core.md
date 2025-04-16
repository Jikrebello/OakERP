## 🧰 EF Core CLI Helper (Windows PowerShell)

To simplify working with migrations and updating your local dev database, a helper script `ef.ps1` is included in the root of the solution.

This script assumes:
- Your **DbContext** lives in `OakERP.Infrastructure`
- Your **Startup project** is `OakERP.WebAPI`

---

### 📜 Commands & Usage

```powershell
# Add a new migration (e.g., when changing entities or relationships)
.\ef.ps1 -action add -name AddCustomersTable

# Remove the last migration (undo bad changes before applying)
.\ef.ps1 -action remove

# Apply all migrations to the database
.\ef.ps1 -action update

# Drop the database (use with caution!)
.\ef.ps1 -action drop
```

---

### 🧪 Real-World Examples

```powershell
# Add a migration after modifying ApplicationUser
.\ef.ps1 -action add -name AddUserTenantRelation

# Rebuild your database completely from scratch
.\ef.ps1 -action drop
.\ef.ps1 -action add -name InitSchema
.\ef.ps1 -action update
```

---

### 🧼 When to Use This

| Task                                      | Use This When                                                  |
|-------------------------------------------|----------------------------------------------------------------|
| `add`                                     | You changed your entities and want to capture the diff         |
| `remove`                                  | You regret your last migration and haven’t run `update` yet    |
| `update`                                  | You want to bring your DB up-to-date with the latest migration |
| `drop`                                    | You're doing a full clean reset during development             |

---

### 🛑 Warnings

- This script is **only for local dev environments**.
- Do **not** use `drop` on production or shared environments.
- If you use Docker, make sure `oakdb` is running before running `update`.
