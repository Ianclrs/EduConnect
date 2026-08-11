\set ON_ERROR_STOP on

-- Create role (if not exists)
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'ciclo') THEN
        CREATE ROLE ciclo WITH LOGIN PASSWORD '1234';
    END IF;
END
$$;

-- Create database (if not exists)
SELECT 'CREATE DATABASE ciclo OWNER ciclo'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'ciclo')\gexec

-- Grant privileges on database
GRANT ALL PRIVILEGES ON DATABASE ciclo TO ciclo;

-- Grant privileges on public schema
\c ciclo
GRANT ALL PRIVILEGES ON SCHEMA public TO ciclo;
