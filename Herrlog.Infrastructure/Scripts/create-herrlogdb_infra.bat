@echo off
setlocal enabledelayedexpansion

REM Assumes script is at Herrlog.Infrastructure\Scripts\
REM Places MDF at Herrlog.Infrastructure\Database

set "SCRIPT_DIR=%~dp0"
for %%I in ("%SCRIPT_DIR%..") do set "INFRA_DIR=%%~fI"
set "DBPATH=%INFRA_DIR%\Database"

if not exist "%DBPATH%" mkdir "%DBPATH%"

echo Target MDF path: "%DBPATH%\HerrlogDb.mdf"
echo.

REM Drop existing DB if attached with same name
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "IF DB_ID('HerrlogDb') IS NOT NULL BEGIN ALTER DATABASE [HerrlogDb] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [HerrlogDb]; END;"

REM Create database pointing MDF/LDF to Infrastructure\Database
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "CREATE DATABASE [HerrlogDb] ON (NAME=N'HerrlogDb', FILENAME=N'%DBPATH%\HerrlogDb.mdf') LOG ON (NAME=N'HerrlogDb_log', FILENAME=N'%DBPATH%\HerrlogDb_log.ldf');"
if errorlevel 1 (
  echo Failed to create database. Make sure SQL Server LocalDB and sqlcmd are installed.
  exit /b 1
)

REM Apply schema (expects schema_herrlogdb.sql next to this script)
sqlcmd -S "(localdb)\MSSQLLocalDB" -d "HerrlogDb" -i "%SCRIPT_DIR%schema_herrlogdb.sql"
if errorlevel 1 (
  echo Failed to apply schema.
  exit /b 1
)

echo Done! MDF created at: "%DBPATH%\HerrlogDb.mdf"
exit /b 0
