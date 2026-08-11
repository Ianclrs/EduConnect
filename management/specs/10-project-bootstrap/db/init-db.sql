-- ============================================================================
-- Ciclo - Database Initialization Script
-- ============================================================================
-- Usage:
--   psql -U postgres -f init-db.sql
--
-- What this script does:
--   1. Creates the edugestor role (user) if it doesn't exist
--   2. Creates the edugestor database if it doesn't exist
--   3. Grants all privileges on the database to the edugestor role
--
-- Prerequisites:
--   - PostgreSQL 16 installed and running
--   - Access as a superuser (default: postgres) via psql
--
-- After running this script, configure appsettings.Development.json:
--   "ConnectionStrings": {
--     "Default": "Host=localhost;Database=edugestor;Username=edugestor;Password=edugestor_dev;Timeout=5;Command Timeout=30"
--   }
-- ============================================================================

-- Safety: exit with error if any statement fails, stop on first error
\set ON_ERROR_STOP on

-- Notify progress
\echo '========================================'
\echo 'Ciclo - Database Initialization'
\echo '========================================'

-- --------------------------------------------------------------------------
-- 1. Create role (user) if it doesn't already exist
-- --------------------------------------------------------------------------
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'edugestor') THEN
        CREATE ROLE edugestor WITH
            LOGIN
            PASSWORD 'edugestor_dev'
            NOSUPERUSER
            INHERIT
            NOCREATEDB
            NOCREATEROLE
            NOREPLICATION;
        \echo '  [OK] Role "edugestor" created.'
    ELSE
        \echo '  [SKIP] Role "edugestor" already exists.'
    END IF;
END
$$;

-- --------------------------------------------------------------------------
-- 2. Create database if it doesn't already exist
-- --------------------------------------------------------------------------
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_database WHERE datname = 'edugestor') THEN
        CREATE DATABASE edugestor
            OWNER = edugestor
            ENCODING = 'UTF8'
            LC_COLLATE = 'en_US.UTF-8'
            LC_CTYPE = 'en_US.UTF-8'
            TEMPLATE = template0;
        \echo '  [OK] Database "edugestor" created.'
    ELSE
        \echo '  [SKIP] Database "edugestor" already exists.'
    END IF
END
$$;

-- --------------------------------------------------------------------------
-- 3. Grant privileges
-- --------------------------------------------------------------------------
GRANT ALL PRIVILEGES ON DATABASE edugestor TO edugestor;

\echo '  [OK] Privileges granted to "edugestor" on database "edugestor".'

-- --------------------------------------------------------------------------
-- 4. Connect to edugestor and set up schema permissions
-- --------------------------------------------------------------------------
\c edugestor

-- Grant schema ownership so EF Core can create tables and run migrations
GRANT ALL ON SCHEMA public TO edugestor;
ALTER SCHEMA public OWNER TO edugestor;

\echo '  [OK] Schema "public" ownership granted to "edugestor".'

\echo '========================================'
\echo 'Database initialization complete.'
\echo 'Connection string:'
\echo '  Host=localhost;Database=edugestor;Username=edugestor;Password=edugestor_dev'
\echo '========================================'
