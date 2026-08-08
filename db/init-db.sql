\set ON_ERROR_STOP on

-- Create role (if not exists)
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'edugestor') THEN
        CREATE ROLE edugestor WITH LOGIN PASSWORD '1234';
    END IF;
END
$$;

-- Create database (if not exists)
SELECT 'CREATE DATABASE edugestor OWNER edugestor'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'edugestor')\gexec

-- Grant privileges on database
GRANT ALL PRIVILEGES ON DATABASE edugestor TO edugestor;

-- Grant privileges on public schema
\c edugestor
GRANT ALL PRIVILEGES ON SCHEMA public TO edugestor;
