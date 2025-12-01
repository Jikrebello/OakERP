#!/usr/bin/env bash
set -e

# The official Postgres image executes *.sh in this folder at init time (once),
# and provides $POSTGRES_USER / $POSTGRES_DB env vars.

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "oakerp" <<-'EOSQL'
  CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
EOSQL

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "oakerp_dev" <<-'EOSQL'
  CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
EOSQL

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "oakerp_test" <<-'EOSQL'
  CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
EOSQL
