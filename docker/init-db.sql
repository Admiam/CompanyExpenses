-- =====================================================
-- Company Expenses - Database Initialization Script
-- This script creates the databases. Migrations and
-- seeding are handled by the application on startup.
-- =====================================================

-- Create the main application database
IF NOT EXISTS (SELECT name
FROM sys.databases
WHERE name = 'company_expenses')
BEGIN
    CREATE DATABASE company_expenses;
    PRINT 'Created database: company_expenses';
END
GO

-- Create the auth database
IF NOT EXISTS (SELECT name
FROM sys.databases
WHERE name = 'company_expenses_auth')
BEGIN
    CREATE DATABASE company_expenses_auth;
    PRINT 'Created database: company_expenses_auth';
END
GO

PRINT '';
PRINT '=====================================================';
PRINT 'Databases created successfully!';
PRINT '=====================================================';
PRINT '';
PRINT 'The applications will apply migrations and seed data';
PRINT 'automatically on first startup.';
PRINT '';
PRINT 'Default Admin Credentials:';
PRINT '  Email: admin@company-expenses.local';
PRINT '  Password: Admin123!';
PRINT '=====================================================';
GO
