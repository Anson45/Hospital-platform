-- Creates the PostgreSQL databases for the Hospital Platform.
-- Runs automatically on first container start via /docker-entrypoint-initdb.d.
--
-- PostgreSQL hosts: Patient/Appointment (HPClinicalDb) and Analytics (HPAnalyticsDb).
-- Identity and Pharmacy/Billing use SQL Server; the AI service uses MongoDB.
-- Database names keep the HP prefix (quoted to preserve casing) for easy identification.

SELECT 'CREATE DATABASE "HPClinicalDb"'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'HPClinicalDb')\gexec

SELECT 'CREATE DATABASE "HPAnalyticsDb"'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'HPAnalyticsDb')\gexec
