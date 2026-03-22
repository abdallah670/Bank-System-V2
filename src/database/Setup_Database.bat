@echo off
echo ============================================
echo   BankSystemV2 Database Setup
echo ============================================
echo.

echo This script will create the BankSystemV2 database.
echo.

set /p SERVER="Enter SQL Server name (default: localhost): "
if "%SERVER%"=="" set SERVER=localhost

echo.
echo Running database scripts...
echo.

sqlcmd -S %SERVER% -E -i "001_Schema.sql"
if %errorlevel% neq 0 (
    echo ERROR: Failed to create schema!
    pause
    exit /b 1
)

sqlcmd -S %SERVER% -E -d BankSystemV2 -i "002_Constraints.sql"
if %errorlevel% neq 0 (
    echo WARNING: Some constraints may already exist.
)

sqlcmd -S %SERVER% -E -d BankSystemV2 -i "003_Indexes.sql"
if %errorlevel% neq 0 (
    echo WARNING: Some indexes may already exist.
)

sqlcmd -S %SERVER% -E -d BankSystemV2 -i "004_Seed_Data.sql"
if %errorlevel% neq 0 (
    echo WARNING: Seed data may already exist.
)

sqlcmd -S %SERVER% -E -d BankSystemV2 -i "005_Phase2_Schema.sql"
if %errorlevel% neq 0 (
    echo WARNING: Phase 2 tables may already exist.
)

echo.
echo ============================================
echo   Setup Complete!
echo ============================================
echo.
echo DEFAULT CREDENTIALS:
echo   Username: admin
echo   Password: Admin@123
echo   Role: SuperAdmin
echo.
echo IMPORTANT: Change the default password after first login!
echo.
pause
