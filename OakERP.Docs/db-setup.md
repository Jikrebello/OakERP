# PostgreSQL + Docker Setup

This guide sets up the local OakERP PostgreSQL database, pgAdmin, and optional Seq instance using the checked-in `docker-compose.yml`.

## What You Get

- PostgreSQL 16 in Docker
- default dev database: `oakerp`
- pgAdmin at `http://localhost:8080`
- optional Seq at `http://localhost:5341`

Current Docker mapping:

- host port `5433` -> container port `5432`

## Prerequisites

- Docker Desktop installed and running
- WSL 2 enabled on Windows if Docker Desktop requires it

## Start The Stack

From the repo root:

```powershell
docker compose up -d
```

This starts:

- `oakdb`: PostgreSQL
- `oakadmin`: pgAdmin

To start Seq too:

```powershell
docker compose --profile seq up -d seq
```

## Connection Details

### PostgreSQL

- Host: `localhost`
- Port: `5433`
- Database: `oakerp`
- Username: `oakadmin`
- Password: `oakpass`

### pgAdmin

- URL: `http://localhost:8080`
- Email: `admin@oakerp.dev`
- Password: `admin123`

## Connect pgAdmin To Postgres

In pgAdmin, add a server with:

- Name: `OakDB`
- Host name/address: `oakdb`
- Port: `5432`
- Username: `oakadmin`
- Password: `oakpass`

Use `oakdb` here because pgAdmin connects inside the Docker network.

## Smoke Check The Database

Optional quick check:

```powershell
docker exec -it oakdb psql "host=localhost port=5432 dbname=oakerp user=oakadmin password=oakpass" -c "select current_user, current_database();"
```

If you connect from the host instead of from inside the container, use port `5433`.

## Reset Everything

To drop the volumes and start fresh:

```powershell
docker compose down -v
docker compose up -d
```

## Optional Seq Logging

OakERP keeps Seq disabled in checked-in config by default.

Start Seq only when needed:

```powershell
docker compose --profile seq up -d seq
```

Then enable the API sink through local config or environment variables, for example:

```powershell
$env:Serilog__Seq__Enabled = "true"
$env:Serilog__Seq__ServerUrl = "http://localhost:5341"
```

Do not commit machine-specific logging overrides or Seq secrets.
