@echo off
REM =====================================================
REM Company Expenses - Docker Startup Script for Windows
REM This script starts all services with a single command
REM =====================================================

echo.
echo ==============================================
echo   Company Expenses - Docker Setup
echo ==============================================
echo.

REM Check if Docker is installed
docker --version >nul 2>&1
if errorlevel 1 (
    echo Error: Docker is not installed.
    echo Please install Docker from https://docker.com
    exit /b 1
)

REM Check if Docker Compose is available
docker compose version >nul 2>&1
if errorlevel 1 (
    echo Error: Docker Compose is not available.
    echo Please install Docker Compose or update Docker.
    exit /b 1
)

echo Starting all services...
echo.

REM Build and start all containers
docker compose up --build -d

if errorlevel 1 (
    echo Error: Failed to start containers.
    exit /b 1
)

echo.
echo ==============================================
echo   Waiting for services to be ready...
echo ==============================================
echo.

echo Waiting for database to be ready...
timeout /t 30 /nobreak >nul

echo Waiting for applications to initialize...
timeout /t 20 /nobreak >nul

echo.
echo ==============================================
echo   Company Expenses is now running!
echo ==============================================
echo.
echo   Frontend App:   http://localhost:3000
echo   Auth Server:    http://localhost:5169
echo   API Server:     http://localhost:5200
echo.
echo   Admin Login Credentials:
echo     Email:    admin@company-expenses.local
echo     Password: Admin123!
echo.
echo   To stop all services:
echo     docker compose down
echo.
echo   To view logs:
echo     docker compose logs -f
echo.
echo ==============================================
