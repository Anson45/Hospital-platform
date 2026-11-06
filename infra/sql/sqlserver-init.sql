-- Creates the SQL Server databases for the Hospital Platform.
-- Executed by the one-shot mssql-init container (see infra/docker-compose.yml).
--
-- SQL Server hosts: Identity (HPIdentityDb) and Pharmacy/Billing (HPPharmacyDb).
-- Patient/Appointment and Analytics use PostgreSQL; the AI service uses MongoDB.

IF DB_ID('HPIdentityDb') IS NULL
BEGIN
    CREATE DATABASE [HPIdentityDb];
END;
GO

IF DB_ID('HPPharmacyDb') IS NULL
BEGIN
    CREATE DATABASE [HPPharmacyDb];
END;
GO

PRINT 'Hospital Platform SQL Server databases are ready.';
GO
