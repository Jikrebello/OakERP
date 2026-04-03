# 🐘 Database Setup with Docker

This guide walks you through setting up the PostgreSQL database server and pgAdmin UI using Docker.  

By the end, you’ll have a `oakerp` database running and visible in pgAdmin for development and inspection.

---

### ✅ 1. Install Required Tools

#### 📥 Docker Desktop
- Download & install from: [https://www.docker.com/products/docker-desktop/](https://www.docker.com/products/docker-desktop/)
- On first launch, Docker will ask to enable **WSL 2** backend (if you're on Windows). Say yes.

#### ⚙️ WSL 2 (Windows only)
- Install WSL 2 via PowerShell:
  ```powershell
  wsl --install
  ```
- Restart your machine if prompted.

---

### ✅ 2. Prepare the Docker Setup

#### 📁 Create Folder Structure (if not already present)
At the root of the solution:

```
/OakERP.Solution
│
├── docker-compose.yml
└── /initdb/
     └── init.sql  ← (optional, see below)
```

#### 📄 `docker-compose.yml`

```yaml
services:
  oakdb:
    image: postgres:16
    container_name: oakdb
    restart: always
    environment:
      POSTGRES_USER: oakadmin
      POSTGRES_PASSWORD: oakpass
      POSTGRES_DB: oakerp
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
      # Optional: Initial SQL (only runs on first volume create)
      - ./initdb:/docker-entrypoint-initdb.d

  oakadmin:
    image: dpage/pgadmin4
    container_name: oakadmin
    restart: always
    environment:
      PGADMIN_DEFAULT_EMAIL: admin@oakerp.dev
      PGADMIN_DEFAULT_PASSWORD: admin123
    ports:
      - "8080:80"
    volumes:
      - pgadmin_data:/var/lib/pgadmin

volumes:
  postgres_data:
  pgadmin_data:
```

---

### ✅ 3. (Optional) Add Initial SQL

If you'd like your database to start with a default table (only happens on first launch):

**📄 `/initdb/init.sql`**
```sql
CREATE TABLE IF NOT EXISTS sample_table (
    id SERIAL PRIMARY KEY,
    name TEXT NOT NULL
);
```

---

### ✅ 4. Start the Database + pgAdmin

From a terminal inside the root folder (`OakERP.Solution`):

```bash
docker-compose up -d
```

This will start:
- `oakdb`: the Postgres database (with DB `oakerp`)
- `oakadmin`: pgAdmin running at http://localhost:8080

---

### ✅ 5. Open pgAdmin

1. Go to: [http://localhost:8080](http://localhost:8080)
2. Login:
   - **Email**: `admin@oakerp.dev`
   - **Password**: `admin123`

---

### ✅ 6. Connect pgAdmin to the Postgres Server

1. Click **"Add New Server"**
2. Under **General** tab:
   - **Name**: `OakDB` (or anything you like)
3. Under **Connection** tab:
   - **Host name/address**: `oakdb`
   - **Port**: `5432`
   - **Username**: `oakadmin`
   - **Password**: `oakpass`
   - ✅ Check “Save Password”
4. Click **Save**

You will now see the `oakerp` database under this server.

---

### 🔁 Resetting the DB (if needed)

If you want to nuke everything and start clean:

```bash
docker-compose down -v
docker-compose up -d
```

This removes volumes (including your DB) and re-runs any init scripts.

---

### Updating the pgAdmin image:

```
docker compose pull oakadmin
docker compose up -d oakadmin
```

---

### Optional local log UI with Seq

OakERP can now send API logs to Seq for local development, but Seq stays disabled by default in
checked-in config.

Start Seq only when you want it:

```bash
docker compose --profile seq up -d seq
```

Then open:
- http://localhost:5341

To point the API at the local Seq instance, enable the sink through configuration instead of
editing checked-in files:

```powershell
$env:Serilog__Seq__Enabled = "true"
$env:Serilog__Seq__ServerUrl = "http://localhost:5341"
```

Do not commit Seq API keys or machine-specific overrides.
